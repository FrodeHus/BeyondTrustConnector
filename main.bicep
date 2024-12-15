type dataCollectionConfig = {
  ruleName: string
  endpointName: string
  workspaceName: string
}

type functionAppConfig = {
  name: string
}

param datacollection dataCollectionConfig
param functionConfig functionAppConfig

module datacollectionModule './datacollection.bicep' = {
  name: 'datacollection'
  params: {
    ruleName: datacollection.ruleName
    endpointName: datacollection.endpointName
    workspaceName: datacollection.workspaceName
  }
}

module functionappModule './functionapp.bicep' = {
  name: 'functionapp'
  params: {
    appName: functionConfig.name
    location: resourceGroup().location
    workspaceId: datacollectionModule.outputs.workspaceResourceId
  }
}
