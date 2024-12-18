using './main.bicep'
param beyondTrustTenant = 'mytenant'
param datacollection  = {
  ruleName: 'dcr-beyondtrust'
  endpointName: 'bt-endpoint'
  workspaceName: 'LAW-BeyondTrust'
}

param functionConfig = {
  name: 'func-btconnect'
  keyvaultName: 'btvault'
  keyvaultSecretName: 'BeyondTrustAPI'
  container: 'frodehus/beyondtrustconnector:latest'
}
