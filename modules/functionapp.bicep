type dataCollectionConfig = {
  endpointImmutableId: string
  endpointUri: string
  workspaceName: string
  beyondTrustTenant: string
}

param dataCollection dataCollectionConfig

@description('The name of the function app that you wish to create.')
param appName string = 'fnapp${uniqueString(resourceGroup().id)}'

@description('Storage Account type')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
])
param storageAccountType string = 'Standard_LRS'

@description('Location for all resources.')
param location string = resourceGroup().location

var functionAppName = appName
var hostingPlanName = appName
var applicationInsightsName = appName
param storageAccountName string = '${uniqueString(resourceGroup().id)}azfunctions'
var functionWorkerRuntime = 'dotnet-isolated'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountType
  }
  kind: 'Storage'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~14'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionWorkerRuntime
        }
        {
          name: 'WORKSPACE_ID'
          value: dataCollection.workspaceName
        }
        {
          name: 'DCR_ID'
          value: dataCollection.endpointImmutableId
        }
        {
          name: 'DCR_ENDPOINT'
          value: dataCollection.endpointUri
        }
        {
          name: 'BEYONDTRUST_TENANT'
          value: dataCollection.beyondTrustTenant
        }
      ]
      netFrameworkVersion: 'v9.0'  
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

resource continuousIntegration 'Microsoft.Web/sites/sourcecontrols@2024-04-01' = {
  parent: functionApp
  name: 'web'
  properties: {
    branch: 'main'
    isManualIntegration: true
    repoUrl: 'https://github.com/FrodeHus/BeyondTrustConnector.git'
  }
}

output managedIdentity string = functionApp.identity.principalId
