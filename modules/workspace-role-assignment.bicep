param roleAssignmentName string

param roleDefinitionId string

param principalId string

param workspaceName string

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: workspaceName
}

resource workspaceRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: roleDefinitionId
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid(roleAssignmentName)
  scope: workspace
  properties: {
    roleDefinitionId: workspaceRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
