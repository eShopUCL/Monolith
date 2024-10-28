#!/bin/zsh

# Load environment variables from .env file
if [ -f .env ]; then
  export $(cat .env | xargs)
else
  echo "ERROR: .env file not found. Please create it and try again."
  exit 1
fi

# Step 2: Authenticate with Azure Container Registry
echo "Logging in to Azure Container Registry..."
az acr login --name ${ACR_NAME}

if [ $? -ne 0 ]; then
  echo "ERROR: Failed to log in to Azure Container Registry."
  exit 1
fi

# Step 3: Use Docker Buildx with Docker Compose to Build All Services for a Specific Platform (linux/amd64)
echo "Building Docker images for all services (linux/amd64)..."
export DOCKER_BUILDKIT=1

# Remove the existing builder and recreate it to avoid append mode issues
docker buildx rm multiarch-builder
docker buildx create --use --name multiarch-builder --driver docker-container

# Perform the multi-platform build using Docker Compose and push to ACR
docker buildx bake --file docker-compose.yml --no-cache --push \
  --set "eshoppublicapi.platform=linux/amd64" --set "eshoppublicapi.tags=${ACR_NAME}.azurecr.io/eshoppublicapi:latest" \
  --set "eshopwebmvc.platform=linux/amd64" --set "eshopwebmvc.tags=${ACR_NAME}.azurecr.io/eshopwebmvc:latest"

if [ $? -ne 0 ]; then
  echo "ERROR: Failed to build and push Docker images for linux/amd64."
  exit 1
fi

# Step 4: Retrieve AKS Credentials
echo "Retrieving AKS credentials..."
az aks get-credentials --resource-group ${RESOURCE_GROUP} --name ${AKS_NAME} --overwrite-existing

# Step 5: Apply Kubernetes YAML Files to Update Deployments and Services
SERVICES=("eshopwebmvc" "eshoppublicapi")
for SERVICE in "${SERVICES[@]}"
do
  YAML_FILE="${SERVICE}.yml"
  if [ -f ${YAML_FILE} ]; then
    echo "Deploying ${SERVICE} using ${YAML_FILE}..."
    kubectl apply -f ${YAML_FILE}
    if [ $? -ne 0 ]; then
      echo "ERROR: Failed to deploy ${SERVICE} using ${YAML_FILE}."
      exit 1
    fi
  else
    echo "WARNING: ${YAML_FILE} does not exist. Skipping deployment for ${SERVICE}."
  fi
done

echo "Deployment completed successfully!"
