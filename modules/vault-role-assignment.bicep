param roleAssignmentName string

param roleDefinitionId string

param principalId string

param keyVaultName string
param secretName string

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2024-04-01-preview' existing = {
    parent: keyVault
    name: secretName
}


resource vaultRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: roleDefinitionId
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(roleAssignmentName)
  scope: keyVaultSecret
  properties: {
    roleDefinitionId: vaultRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'    
  }
}
