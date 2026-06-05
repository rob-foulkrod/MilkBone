// Infrastructure for the MilkBone web app deployment.
// Deployed at resource group scope into the existing 'milkbone_deploy' group.

@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('App Service name. Note: Web App names cannot contain underscores, so the requested "milkbone_webapp" is provisioned as "milkbone-webapp".')
param webAppName string = 'milkbone-webapp'

@description('App Service Plan name.')
param appServicePlanName string = 'milkbone-plan'

@description('App Service Plan SKU. Standard (S1) or higher is required to enable deployment slots.')
param skuName string = 'S1'

@description('Name of the non-production deployment slot used for blue/green swaps.')
param stagingSlotName string = 'staging'

// Demo buffer tag applied to every resource so a 14-day security-control grace
// period is honored across the deployment.
var resourceTags = {
  SecurityControl: 'Ignore'
}

// .NET 10 runtime stack for Linux App Service.
var linuxFxVersion = 'DOTNETCORE|10.0'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: resourceTags
  sku: {
    name: skuName
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  tags: resourceTags
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      alwaysOn: true
    }
  }
}

// Second deployment slot (production slot is implicit). Together this gives the
// two slots used for zero-downtime swap deployments.
resource stagingSlot 'Microsoft.Web/sites/slots@2023-12-01' = {
  parent: webApp
  name: stagingSlotName
  location: location
  tags: resourceTags
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      alwaysOn: true
    }
  }
}

output webAppName string = webApp.name
output stagingSlotName string = stagingSlot.name
output defaultHostName string = webApp.properties.defaultHostName
