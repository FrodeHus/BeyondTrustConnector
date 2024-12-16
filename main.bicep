type dataCollectionConfig = {
  ruleName: string
  endpointName: string
  workspaceName: string
}

type functionAppConfig = {
  name: string
  keyvaultName: string
}

param beyondTrustTenant string
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
    dataCollection: {
      workspaceName: datacollectionModule.outputs.workspaceResourceId
      endpointImmutableId: datacollectionModule.outputs.dcrImmutableId
      endpointUri: datacollectionModule.outputs.logsIngestionEndpoint
      beyondTrustTenant: beyondTrustTenant
    }
  }
}

var principalId = functionappModule.outputs.managedIdentity

module vaultSecretUserRoleAssignment './modules/vault-role-assignment.bicep' = {
  name: 'vaultSecretUserRoleAssignment'
  params: {
    roleAssignmentName: '${uniqueString(functionConfig.name)}-keyvault-reader-role-assignment'
    roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secret User
    principalId: principalId
    keyVaultName: functionConfig.keyvaultName
  }
}

module workspaceReaderRoleAssignment './modules/workspace-role-assignment.bicep' = {
  name: 'workspaceReaderRoleAssignment'
  params: {
    roleAssignmentName: '${uniqueString(functionConfig.name)}-workspace-reader-role-assignment'
    roleDefinitionId: '73c42c96-874c-492b-b04d-ab87d138a893' // Log Analytics Reader
    principalId: principalId
    workspaceName: datacollection.workspaceName
  }
}

module workspaceMetricPublisherRoleAssignment './modules/workspace-role-assignment.bicep' = {
  name: 'workspaceMetricPublisherRoleAssignment'
  params: {
    roleAssignmentName: '${uniqueString(functionConfig.name)}-workspace-metric-publisher-role-assignment'
    roleDefinitionId: '3913510d-42f4-4e42-8a64-420c390055eb' // Log Analytics Reader
    principalId: principalId
    workspaceName: datacollection.workspaceName
  }
}
