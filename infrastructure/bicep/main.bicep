@description('Cosmos DB account name')
param cosmosAccountName string = 'cosmos-db-mw-ai-26' //'cosmos-${uniqueString(resourceGroup().id)}'

@description('Location for the Cosmos DB account.')
param location string = resourceGroup().location

@description('The name for the SQL API database')
param cosmosDatabaseName string = 'ai-experiments-db'

@description ('Whether to deploy the SQL server and database. This is set to false by default as SQL db deployment requires manual steps to enable the free tier.')
param deploySqlServer bool = false

@description('The name of the SQL logical server.')
param sqlServerName string = uniqueString('sql', resourceGroup().id)

@description('The name of the SQL Database.')
param sqlDatabaseName string = 'SampleDB'

@description('The administrator username of the SQL logical server.')
param sqlAdministratorLogin string

@description('The administrator password of the SQL logical server.')
@secure()
param sqlAdministratorPassword string

/* Deploy Cosmos DB */
resource account 'Microsoft.DocumentDB/databaseAccounts@2024-12-01-preview' = {
  name: toLower(cosmosAccountName)
  location: location
    tags: {
    defaultExperience: 'Core (SQL)'
    'hidden-workload-type': 'Development/Testing'
  }
  properties: {
    enableFreeTier: true
    databaseAccountOfferType: 'Standard'
    capacityMode: 'Provisioned'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
      }
    ]
    capacity: {
      totalThroughputLimit: 1000
    }
  }
}

/* Deploy Cosmos database*/
resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-12-01-preview' = {
  parent: account
  name: cosmosDatabaseName
  properties: {
    resource: {
      id: cosmosDatabaseName
    }
    options: {
      throughput: 1000
    }
  }
}

/* Deploy SQL Server and Database if deploySqlServer is true */
resource sqlServer 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: sqlServerName
  location: location
  kind: 'v12.0'
  properties: {
     administratorLogin: sqlAdministratorLogin
     administratorLoginPassword: sqlAdministratorPassword
     minimalTlsVersion: '1.2'
     publicNetworkAccess: 'Enabled'
     restrictOutboundNetworkAccess: 'Disabled'
     version: '12.0'
   }
}
resource sqlDB 'Microsoft.Sql/servers/databases@2024-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  kind: 'v12.0,user,vcore,serverless,freelimit'
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    readScale: 'Disabled'
    autoPauseDelay: 60
    availabilityZone: 'NoPreference'
    zoneRedundant: false
    freeLimitExhaustionBehavior: 'AutoPause'
  }
}

