param ruleName string
param endpointName string
param workspaceName string
param principalId string

resource law 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: workspaceName
}

output workspaceId string = law.properties.customerId

resource dataCollectionEndpoint 'Microsoft.Insights/dataCollectionEndpoints@2023-03-11' = {
  name: endpointName
  location: resourceGroup().location
  properties: {
    networkAcls: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource Custom_Table_BeyondTrustLicenseUsage_CL 'Microsoft.OperationalInsights/workspaces/tables@2022-10-01' = {
  parent: law
  name: 'BeyondTrustLicenseUsage_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BeyondTrustLicenseUsage_CL'
      columns: [
          {
            name: 'TimeGenerated'
            type: 'datetime'
          }
          {
            name: 'Name'
            type: 'string'
          }
          {
            name: 'HostnameOrIp'
            type: 'string'
          }
          {
            name: 'Jumpoint'
            type: 'string'
          }
          {
            name: 'License'
            type: 'boolean'
          }
          {
            name: 'JumpMethod'
            type: 'string'
          }
          {
            name: 'JumpGroup'
            type: 'string'
          }
      ]
    }
    retentionInDays: 30
  }
}

resource Custom_Table_BeyondTrustVaultActivity_CL 'Microsoft.OperationalInsights/workspaces/tables@2022-10-01' = {
  parent: law
  name: 'BeyondTrustVaultActivity_CL'
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
  parent: law
  name: 'BeyondTrustEvents_CL'
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
  parent: law
  name: 'BeyondTrustAccessSession_CL'
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
          name: 'JumpGroupId'
          type: 'int'
        }
        {
          name: 'Jumpoint'
          type: 'string'
        }
        {
          name: 'UserDetails'
          type: 'dynamic'
        }
        {
          name: 'Events'
          type: 'dynamic'
        }
        {
          name: 'FileTransferCount'
          type: 'int'
        }
        {
          name: 'FileMoveCount'
          type: 'int'
        }
        {
          name: 'FileDeleteCount'
          type: 'int'
        }
        {
          name: 'ChatDownloadUrl'
          type: 'string'
        }
        {
          name: 'ChatViewUrl'
          type: 'string'
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
        'Custom-BeyondTrustLicenseUsage_CL': {
        columns: [
          {
            name: 'TimeGenerated'
            type: 'datetime'
          }
          {
            name: 'JumpItemName'
            type: 'string'
          }
          {
            name: 'HostnameOrIp'
            type: 'string'
          }
          {
            name: 'Jumpoint'
            type: 'string'
          }
          {
            name: 'License'
            type: 'boolean'
          }
          {
            name: 'JumpMethod'
            type: 'string'
          }
          {
            name: 'JumpGroup'
            type: 'string'
          }
        ]
      }
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
            name: 'JumpGroupId'
            type: 'int'
          }
          {
            name: 'Jumpoint'
            type: 'string'
          }
          {
            name: 'UserDetails'
            type: 'dynamic'
          }
          {
            name: 'Events'
            type: 'dynamic'
          }
          {
            name: 'ChatDownloadUrl'
            type: 'string'
          }
          {
            name: 'ChatViewUrl'
            type: 'string'
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
          {
            name: 'FileTransferCount'
            type: 'int'
          }
          {
            name: 'FileMoveCount'
            type: 'int'
          }
          {
            name: 'FileDeleteCount'
            type: 'int'
          }
        ]
      }
    }
    dataSources: {}
    destinations: {
      logAnalytics: [
        {
          workspaceResourceId: law.id
          name: 'beyondTrustWorkspace'
        }
      ]
    }
    dataFlows: [
        {
        streams: [
          'Custom-BeyondTrustLicenseUsage_CL'
        ]
        destinations: [
          'beyondTrustWorkspace'
        ]
        transformKql: 'source\n| extend TimeGenerated=now()\n|project-rename Name=JumpItemName\n'
        outputStream: 'Custom-BeyondTrustLicenseUsage_CL'
      }
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

resource metricPublisherRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-05-01-preview' existing = {
  name: '3913510d-42f4-4e42-8a64-420c390055eb'
}

resource metricPublisherRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  name: guid('metricPublisherRoleAssignment')
  scope: dataCollectionRule
  properties: {
    roleDefinitionId: metricPublisherRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

output dcrImmutableId string = dataCollectionRule.properties.immutableId
output logsIngestionEndpoint string = dataCollectionEndpoint.properties.logsIngestion.endpoint
