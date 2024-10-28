#!/bin/bash

# Stop Script - stop_aks.sh
# This script will stop the AKS cluster

RESOURCE_GROUP="eShopResourceGroup"
AKS_CLUSTER_NAME="eShopCluster"

# Stop AKS Cluster
echo "Stopping AKS Cluster..."
az aks stop --name $AKS_CLUSTER_NAME --resource-group $RESOURCE_GROUP

echo "AKS Cluster is stopped"
