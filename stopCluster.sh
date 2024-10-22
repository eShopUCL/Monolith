#!/bin/bash

# Stop Script - stop_aks.sh
# This script will stop the AKS cluster and delete load balancers to minimize costs.

RESOURCE_GROUP="eShopResourceGroup"
AKS_CLUSTER_NAME="eShopCluster"

# Stop AKS Cluster
echo "Stopping AKS Cluster..."
az aks stop --name $AKS_CLUSTER_NAME --resource-group $RESOURCE_GROUP

# Delete Load Balancers
echo "Deleting Load Balancers..."
LOAD_BALANCERS=$(az network lb list --resource-group $RESOURCE_GROUP --query "[].name" -o tsv)

for LB in $LOAD_BALANCERS; do
  echo "Deleting load balancer: $LB"
  az network lb delete --name $LB --resource-group $RESOURCE_GROUP
  echo "Deleted load balancer: $LB"
done

echo "AKS Cluster is stopped and load balancers are deleted."
