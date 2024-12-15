param ruleName string
param endpointName string
param workspaceName string

var workspaceResourceId = resourceId('Microsoft.OperationalInsights/workspaces', workspaceName)
output workspaceResourceId string = workspaceResourceId

resource dataCollectionEndpoint 'Microsoft.Insights/dataCollectionEndpoints@2023-03-11' = {
  name: endpointName
  location: resourceGroup().location
  properties: {
    networkAcls: {
      publicNetworkAccess: 'Enabled'
    }
  }
}



resource Custom_Table_BeyondTrustVaultActivity_CL 'Microsoft.OperationalInsights/workspaces/tables@2022-10-01' = {
  name: '${workspaceName}/BeyondTrustVaultActivity_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BeyondTrustVaultActivity_CL'
      columns: [
        {
            name: 'TimeGenerated'
            type: 'datetime'
          }
          {
            name: 'SessionId'
            type: 'string'
          }
          {
            name: 'EventType'
            type: 'string'
          }
          {
            name: 'VaultAccountId'
            type: 'int'
          }
          {
            name: 'User'
            type: 'string'
          }
          {
            name: 'UserId'
            type: 'int'
          }
      ]
    }
    retentionInDays: 30
  }
}

resource Custom_Table_BeyondTrustEvents_CL 'Microsoft.OperationalInsights/workspaces/tables@2022-10-01' = {
  name: '${workspaceName}/BeyondTrustEvents_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BeyondTrustEvents_CL'
      columns: [
        {
          name: 'TimeGenerated'
          type: 'datetime'
        }
        {
          name: 'Hostname'
          type: 'string'
        }
        {
          name: 'CorrelationId'
          type: 'int'
        }
        {
          name: 'SiteId'
          type: 'int'
        }
        {
          name: 'EventType'
          type: 'string'
        }
        {
          name: 'AdditionalData'
          type: 'dynamic'
        }
      ]
    }
    retentionInDays: 30
  }
}

resource Custom_Table_BeyondTrustAccessSession_CL 'Microsoft.OperationalInsights/workspaces/tables@2022-10-01' = {
  name: '${workspaceName}/BeyondTrustAccessSession_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BeyondTrustAccessSession_CL'
      columns: [
        {
          name: 'TimeGenerated'
          type: 'datetime'
        }
        {
          name: 'EndTime'
          type: 'datetime'
        }
        {
          name: 'SessionId'
          type: 'string'
        }
        {
          name: 'SessionType'
          type: 'string'
        }
        {
          name: 'JumpItemId'
          type: 'int'
        }
        {
          name: 'JumpItemAddress'
          type: 'string'
        }
        {
          name: 'JumpGroup'
          type: 'string'
        }
        {
          name: 'Jumpoint'
          type: 'string'
        }
        {
          name: 'UserDetails'
          type: 'dynamic'
        }
      ]
    }
    retentionInDays: 30
  }
}

resource dataCollectionRule 'Microsoft.Insights/dataCollectionRules@2023-03-11' = {
  name: ruleName
  location: resourceGroup().location
  properties: {
    dataCollectionEndpointId: dataCollectionEndpoint.id
    streamDeclarations: {
      'Custom-BeyondTrustEvents_CL': {
        columns: [
          {
            name: 'Timestamp'
            type: 'datetime'
          }
          {
            name: 'Hostname'
            type: 'string'
          }
          {
            name: 'CorrelationId'
            type: 'int'
          }
          {
            name: 'SiteId'
            type: 'int'
          }
          {
            name: 'EventType'
            type: 'string'
          }
          {
            name: 'AdditionalData'
            type: 'dynamic'
          }
        ]
      }
      'Custom-BeyondTrustAccessSession_CL': {
        columns: [
          {
            name: 'StartTime'
            type: 'datetime'
          }
          {
            name: 'EndTime'
            type: 'datetime'
          }
          {
            name: 'SessionId'
            type: 'string'
          }
          {
            name: 'SessionType'
            type: 'string'
          }
          {
            name: 'JumpItemId'
            type: 'int'
          }
          {
            name: 'JumpItemAddress'
            type: 'string'
          }
          {
            name: 'JumpGroup'
            type: 'string'
          }
          {
            name: 'Jumpoint'
            type: 'string'
          }
          {
            name: 'UserDetails'
            type: 'dynamic'
          }
        ]
      }
      'Custom-BeyondTrustVaultActivity_CL': {
        columns: [
          {
            name: 'Timestamp'
            type: 'datetime'
          }
          {
            name: 'SessionId'
            type: 'string'
          }
          {
            name: 'EventType'
            type: 'string'
          }
          {
            name: 'VaultAccountId'
            type: 'int'
          }
          {
            name: 'User'
            type: 'string'
          }
          {
            name: 'UserId'
            type: 'int'
          }
        ]
      }
    }
    dataSources: {}
    destinations: {
      logAnalytics: [
        {
          workspaceResourceId: workspaceResourceId
          name: 'beyondTrustWorkspace'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Custom-BeyondTrustEvents_CL'
        ]
        destinations: [
          'beyondTrustWorkspace'
        ]
        transformKql: 'source\n| project-rename TimeGenerated=Timestamp\n'
        outputStream: 'Custom-BeyondTrustEvents_CL'
      }
      {
        streams: [
          'Custom-BeyondTrustAccessSession_CL'
        ]
        destinations: [
          'beyondTrustWorkspace'
        ]
        transformKql: 'source\n| project-rename TimeGenerated=StartTime\n'
        outputStream: 'Custom-BeyondTrustAccessSession_CL'
      }
      {
        streams: [
          'Custom-BeyondTrustVaultActivity_CL'
        ]
        destinations: [
          'beyondTrustWorkspace'
        ]
        transformKql: 'source\n| project-rename TimeGenerated=Timestamp\n'
        outputStream: 'Custom-BeyondTrustVaultActivity_CL'
      }
    ]
  }
  dependsOn:[
      Custom_Table_BeyondTrustVaultActivity_CL
  ]
}
