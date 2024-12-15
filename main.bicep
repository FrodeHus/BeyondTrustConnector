type dataCollectionConfig = {
  ruleName: string
  endpointName: string
  workspaceName: string
}

type functionAppConfig = {
  name: string
  keyvaultName: string
}

param datacollection dataCollectionConfig
param functionConfig functionAppConfig

module datacollectionModule './modules/datacollection.bicep' = {
  name: 'datacollection'
  params: {
    ruleName: datacollection.ruleName
    endpointName: datacollection.endpointName
    workspaceName: datacollection.workspaceName
  }
}

module functionappModule './modules/functionapp.bicep' = {
  name: 'functionapp'
  params: {
    appName: functionConfig.name
    location: resourceGroup().location
    workspaceId: datacollectionModule.outputs.workspaceResourceId
  }
}

var principalId = functionappModule.outputs.managedIdentity

module vaultSecretUserRoleAssignment './modules/role-assignment.bicep' = {
  name: 'roleAssignment'
  params: {
    roleAssignmentName: '${uniqueString(functionConfig.name)}-keyvault-reader-role-assignment'
    roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secret User
    principalId: principalId
    keyVaultName: functionConfig.keyvaultName
  }
}
