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
var applicationInsightsName = appName
param storageAccountName string = '${uniqueString(resourceGroup().id)}azfunctions'
param container string = 'frodehus/beyondtrustconnector:v1.2'
param keyvaultName string
param userAssignedIdentityId string
param clientId string

resource managedEnvironment 'Microsoft.App/managedEnvironments@2024-10-02-preview' = {
  name: appName
  location: location
  properties:{
    workloadProfiles:[
        {
            name: 'Consumption'
            workloadProfileType: 'Consumption'
        }
    ]
  }
  
}
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

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux,container,azurecontainerapps'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: managedEnvironment.id
    siteConfig: {
        linuxFxVersion:'DOCKER|${container}'
        use32BitWorkerProcess: false
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
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
        {
            name: 'KEYVAULT_NAME'
            value:keyvaultName
        }
        {
            name: 'PRINCIPAL_ID' 
            value: clientId
        }
        {
            name: 'AZURE_CLIENT_ID'
            value: clientId
        }
      ]
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    resourceConfig: {
        cpu:1
        memory:'2Gi'
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

