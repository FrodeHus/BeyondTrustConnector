using './main.bicep'

param datacollection  = {
  ruleName: 'dcr-beyondtrust'
  endpointName: 'beyondtrust-endpoint'
  workspaceName: 'LAW-BT'
}

param functionConfig = {
  name: 'beyondtrust-function'
  keyvaultName: 'beyondtrust-keyvault'
}
