//////////////////////////////////////////////////////////////////////
//// Parameters
//////////////////////////////////////////////////////////////////////

param location string = resourceGroup().location
param resourceNamePostfix string = uniqueString(resourceGroup().id)
param storageAccountName string = 'str${resourceNamePostfix}'
param blobContainerName string
param mediaServiceName string = 'ams${resourceNamePostfix}'
param videoIndexerName string = 'avam-${resourceNamePostfix}'
param managedIdForMediaServiceName string = 'id-${mediaServiceName}'
param managedIdForVideoIndexerName string = 'id-${videoIndexerName}'
param apiFunctionAppName string = 'func-${resourceNamePostfix}-api'
param backendFunctionAppName string = 'func-${resourceNamePostfix}-backend'
param appInsightsName string = 'appi-${resourceNamePostfix}'
param appServicePlanName string = 'plan-${resourceNamePostfix}'
param staticWebAppName string = 'stapp-${resourceNamePostfix}'
param staticWebAppLocation string = 'eastasia'
param staticWebAppSku string = 'Standard'

//////////////////////////////////////////////////////////////////////
//// Modules
//////////////////////////////////////////////////////////////////////

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageAccountName: storageAccountName
    blobContainerNames: [ blobContainerName ]
  }
}

module videoIndexer 'modules/video-indexer.bicep' = {
  name: 'videoIndexer'
  params: {
    storageAccountName: storageAccountName
    location: location
    mediaServiceName: mediaServiceName
    videoIndexerName: videoIndexerName
    managedIdForMediaServiceName: managedIdForMediaServiceName
    managedIdForVideoIndexerName: managedIdForVideoIndexerName
  }
  dependsOn: [ storage ]
}

module functionAppsForApi 'modules/functions.bicep' = {
  name: 'functionAppsForApi'
  params: {
    storageAccountName: storageAccountName
    blobContainerName: blobContainerName
    videoIndexerName: videoIndexerName
    location: location
    appInsightsName: appInsightsName
    appServicePlanName: appServicePlanName
    functionAppName: apiFunctionAppName
  }
  dependsOn: [ storage, videoIndexer ]
}

module functionAppsForBackend 'modules/functions.bicep' = {
  name: 'functionAppsForBackend'
  params: {
    storageAccountName: storageAccountName
    blobContainerName: blobContainerName
    videoIndexerName: videoIndexerName
    location: location
    appInsightsName: appInsightsName
    appServicePlanName: appServicePlanName
    functionAppName: backendFunctionAppName
  }
  dependsOn: [ storage, videoIndexer ]
}

module staticWebApp 'modules/static-webapps.bicep' = {
  name: 'staticWebApp'
  params: {
    functionAppName: apiFunctionAppName
    staticWebAppName: staticWebAppName
    staticWebAppLocation: staticWebAppLocation
    staticWebAppSku: staticWebAppSku
  }
  dependsOn: [ functionAppsForApi ]
}

//////////////////////////////////////////////////////////////////////
//// Outputs
//////////////////////////////////////////////////////////////////////

output tenantId string = tenant().tenantId
output subscriptionId string = subscription().subscriptionId
output storageAccountName string = storageAccountName
output apiFunctionAppName string = apiFunctionAppName
output backendFunctionAppName string = backendFunctionAppName
output staticWebAppName string = staticWebAppName
output staticWebAppHostName string = staticWebApp.outputs.staticWebAppHostName
output videoIndexerResourceId string = videoIndexer.outputs.videoIndexerResourceId
output videoIndexerLocation string = videoIndexer.outputs.videoIndexerLocation
output videoIndexerAccountId string = videoIndexer.outputs.videoIndexerAccountId
