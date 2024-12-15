using './main.bicep'

param datacollection  = {
  ruleName: 'dcr-beyondtrust'
  endpointName: 'beyondtrust-endpoint'
  workspaceName: 'LAW-BeyondTrust-Test'
}

param functionConfig = {
  name: 'beyondtrust-function'
  keyvaultName: 'beyondtrust-keyvault'
}
