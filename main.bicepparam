using './main.bicep'

// Specify your BeyondTrust tenant name
param beyondTrustApplianceUrl = 'https://mytenant.beyondtrustcloud.com'

// Data collection configuration
param datacollection = {
  ruleName: 'dcr-beyondtrust'       // Name of the Data Collection Rule
  endpointName: 'bt-endpoint'       // Name of the Data Collection Endpoint
  workspaceName: 'LAW-BeyondTrust'  // Name of the Log Analytics Workspace
}

// Azure Function App configuration
param functionConfig = {
  name: 'func-btconnect'                                // Name of the Function App
  keyvaultName: 'btvault'                               // Name of the Key Vault containing the API credentials
  keyvaultSecretName: 'BeyondTrustAPI'                  // Name of the secret in Key Vault with BeyondTrust API credentials
  container: 'frodehus/beyondtrustconnector:latest'     // Docker image for the Function App
}
