#!/bin/bash

# Set variables
RESOURCE_GROUP="eShopResourceGroup"
CLUSTER_NAME="eShopCluster"

echo "Starting AKS cluster..."

# Start the AKS cluster
az aks start --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP

# Wait for AKS cluster to be fully up and running
echo "Waiting for AKS cluster to be ready..."
while true; do
  STATUS=$(az aks show --name $CLUSTER_NAME --resource-group $RESOURCE_GROUP --query "powerState.code" -o tsv)
  
  if [[ "$STATUS" == "Running" ]]; then
    echo "AKS cluster is now running."
    break
  fi

  echo "Cluster not yet ready. Waiting 15 seconds before checking again..."
  sleep 15
done

# Retrieve credentials
echo "Getting cluster credentials..."
az aks get-credentials --resource-group $RESOURCE_GROUP --name $CLUSTER_NAME --overwrite-existing

# Re-deploy services to recreate Load Balancers
echo "Re-deploying services to recreate Load Balancers..."
kubectl apply -f eshopwebmvc.yml
kubectl apply -f eshoppublicapi.yml
kubectl apply -f sqlserver.yml

echo "Cluster and load balancers have been started successfully."
