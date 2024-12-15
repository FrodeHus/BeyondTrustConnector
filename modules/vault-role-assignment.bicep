param roleAssignmentName string

param roleDefinitionId string

param principalId string

param keyVaultName string

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

resource vaultRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: roleDefinitionId
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(roleAssignmentName)
  scope: keyVault
  properties: {
    roleDefinitionId: vaultRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
