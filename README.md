# BeyondTrustConnector

Uses function app to pull reports from BeyondTrust API and push to a Azure Log Analytics Workspace.

Requires:
- BeyondTrust API enabled
- BeyondTrust API credentials (added to KeyVault as `BeyondTrustAPI`)

## Install

Pre-requisites:
- Existing Log Analytics Workspace
- Key Vault with BeyondTrust API credentials secret

Update `main.bicepparam` to match your environment and run: `az deployment group create --resource-group <group-name> --template-file main.bicep --parameters main.bicepparam`

This will deploy:
- Data Collection Endpoint
- Data Collection Rules
- Custom tables for LAW
- Azure Function App
  - Managed identity with role assignments to read keyvault secrets and query workspace
 
