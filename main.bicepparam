using './main.bicep'

param datacollection  = {
  ruleName: 'dcr-beyondtrust'
  endpointName: 'beyondtrust-endpoint'
  workspaceName: 'LAW-BT-Test'
}

param functionConfig = {
  name: 'beyondtrust-function'
  keyvaultName: 'beyondtrust-vault'
}
