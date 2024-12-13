param dataCollectionRules_dcr_beyondtrust_name string = 'dcr-beyondtrust'
param dataCollectionEndpoints_ve_secops_endpoint_externalid string = '/subscriptions/2a51bb28-48aa-4c1e-9f36-93e91cc453d4/resourceGroups/RSG-WEU-SOC-VE-Analytics/providers/Microsoft.Insights/dataCollectionEndpoints/ve-secops-endpoint'
param workspaces_law_ve_secops_analytics_externalid string = '/subscriptions/2a51bb28-48aa-4c1e-9f36-93e91cc453d4/resourceGroups/rsg-weu-soc-ve-analytics/providers/microsoft.operationalinsights/workspaces/law-ve-secops-analytics'

param workspaces_LAW_VE_SecOps_Analytics_name string = 'LAW-VE-SecOps-Analytics'

resource Custom_Table_BeyondTrustVaultActivity_CL 'Microsoft.OperationalInsights/workspaces/tables@2022-10-01' = {
  name: '${workspaces_LAW_VE_SecOps_Analytics_name}/BeyondTrustVaultActivity_CL'
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


resource dataCollectionRules_dcr_beyondtrust_name_resource 'Microsoft.Insights/dataCollectionRules@2023-03-11' = {
  name: dataCollectionRules_dcr_beyondtrust_name
  location: 'northeurope'
  properties: {
    dataCollectionEndpointId: dataCollectionEndpoints_ve_secops_endpoint_externalid
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
          workspaceResourceId: workspaces_law_ve_secops_analytics_externalid
          name: '5a11abe40bfa40278b3cfec620c783f8'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Custom-BeyondTrustEvents_CL'
        ]
        destinations: [
          '5a11abe40bfa40278b3cfec620c783f8'
        ]
        transformKql: 'source\n| project-rename TimeGenerated=Timestamp\n'
        outputStream: 'Custom-BeyondTrustEvents_CL'
      }
      {
        streams: [
          'Custom-BeyondTrustAccessSession_CL'
        ]
        destinations: [
          '5a11abe40bfa40278b3cfec620c783f8'
        ]
        transformKql: 'source\n| project-rename TimeGenerated=StartTime\n'
        outputStream: 'Custom-BeyondTrustAccessSession_CL'
      }
      {
        streams: [
          'Custom-BeyondTrustVaultActivity_CL'
        ]
        destinations: [
          '5a11abe40bfa40278b3cfec620c783f8'
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
