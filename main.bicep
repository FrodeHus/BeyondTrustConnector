param dataCollectionRules_dcr_beyondtrust_name string = 'dcr-beyondtrust'
param dataCollectionEndpoints_ve_secops_endpoint_externalid string = '/subscriptions/2a51bb28-48aa-4c1e-9f36-93e91cc453d4/resourceGroups/RSG-WEU-SOC-VE-Analytics/providers/Microsoft.Insights/dataCollectionEndpoints/ve-secops-endpoint'
param workspaces_law_ve_secops_analytics_externalid string = '/subscriptions/2a51bb28-48aa-4c1e-9f36-93e91cc453d4/resourceGroups/rsg-weu-soc-ve-analytics/providers/microsoft.operationalinsights/workspaces/law-ve-secops-analytics'

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
        transformKql: 'source\n| project-rename TimeGenerated=Timestamp\n| extend AdditionalData=Details\n'
        outputStream: 'Custom-BeyondTrustEvents_CL'
      }
    ]
  }
}
