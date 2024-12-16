type dataCollectionConfig = {
  ruleName: string
  endpointName: string
  workspaceName: string
}

type functionAppConfig = {
  name: string
  keyvaultName: string
  container: string
}

param beyondTrustTenant string
param datacollection dataCollectionConfig
param functionConfig functionAppConfig

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: '${functionConfig.name}-${uniqueString(resourceGroup().name)}'
  location: resourceGroup().location
}

module datacollectionModule './modules/datacollection.bicep' = {
  name: 'datacollection'
  params: {
      principalId: userAssignedIdentity.properties.principalId
    ruleName: datacollection.ruleName
    endpointName: datacollection.endpointName
    workspaceName: datacollection.workspaceName
  }
}

module functionappModule './modules/functionapp.bicep' = {
  name: 'functionapp'
  params: {
    appName: '${functionConfig.name}-${uniqueString(resourceGroup().name)}'
    location: resourceGroup().location
    keyvaultName: functionConfig.keyvaultName
    userAssignedIdentityId: userAssignedIdentity.id
    clientId: userAssignedIdentity.properties.clientId
    container: functionConfig.container
    dataCollection: {
      workspaceName: datacollectionModule.outputs.workspaceId
      endpointImmutableId: datacollectionModule.outputs.dcrImmutableId
      endpointUri: datacollectionModule.outputs.logsIngestionEndpoint
      beyondTrustTenant: beyondTrustTenant
    }
  }
}

module vaultSecretUserRoleAssignment './modules/vault-role-assignment.bicep' = {
  name: 'vaultSecretUserRoleAssignment'
  params: {
    roleAssignmentName: '${uniqueString(functionConfig.name)}-keyvault-reader-role-assignment'
    roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secret User
    principalId: userAssignedIdentity.properties.principalId
    keyVaultName: functionConfig.keyvaultName
  }
}

module workspaceReaderRoleAssignment './modules/workspace-role-assignment.bicep' = {
  name: 'workspaceReaderRoleAssignment'
  params: {
    roleAssignmentName: '${uniqueString(functionConfig.name)}-workspace-reader-role-assignment'
    roleDefinitionId: '73c42c96-874c-492b-b04d-ab87d138a893' // Log Analytics Reader
    principalId: userAssignedIdentity.properties.principalId
    workspaceName: datacollection.workspaceName
  }
}
 