@description('Cosmos DB account name')
param accountName string = 'cosmos-db-mw-ai-26' //'cosmos-${uniqueString(resourceGroup().id)}'

@description('Location for the Cosmos DB account.')
param location string = resourceGroup().location

@description('The name for the SQL API database')
param databaseName string = 'ai-experiments-db'

@description('The name for the AI Foundry resource')
param aiFoundryName string = 'ai-mw-foundry-2026' //'aifoundry${uniqueString(resourceGroup().id)}'

// AI Foundry parameters
@description('The name if the default AI Foundry project')
param aiFoundryProjectName string = 'ai-experiments'

@description('Model provider')
param modelPublisherFormat string = 'GlobalStandard'

param modelName string = 'gpt-4o-mini'
param embeddingModelName string = 'text-embedding-3-small'

@description('Version of the model to deploy')
param modelVersion string = '2025-08-07'

@description('Model deployment SKU name')
param skuName string = 'GlobalStandard'
//@description('Content filter policy name')
param contentFilterPolicyName string = 'aiFoundry_Microsoft_DefaultV2'
@description('Model deployment capacity')
param capacity int = 1

/* SQL db requires manaul deployment to get free tier, so not included */

/* Deploy Cosmos DB */

resource account 'Microsoft.DocumentDB/databaseAccounts@2024-12-01-preview' = {
  name: toLower(accountName)
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

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-12-01-preview' = {
  parent: account
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
    options: {
      throughput: 1000
    }
  }
}

/* Deploy AI Foundry and models */
resource aiFoundry_resource 'Microsoft.CognitiveServices/accounts@2025-06-01' = {
  name: aiFoundryName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    apiProperties: {}
    customSubDomainName: aiFoundryName
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    allowProjectManagement: true
    defaultProject: aiFoundryProjectName
    associatedProjects: [
      aiFoundryProjectName
    ]
    publicNetworkAccess: 'Enabled'
  }
}

resource aiFoundry_Default 'Microsoft.CognitiveServices/accounts/defenderForAISettings@2025-06-01' = {
  parent: aiFoundry_resource
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
}

resource aiFoundry_ai_experiments 'Microsoft.CognitiveServices/accounts/projects@2025-06-01' = {
  parent: aiFoundry_resource
  name: aiFoundryProjectName
  location: location
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    description: 'Default project created with the resource'
    displayName: aiFoundryProjectName
  }
}

/*
resource aiFoundry_Microsoft_Default 'Microsoft.CognitiveServices/accounts/raiPolicies@2025-06-01' = {
  parent: aiFoundry_resource
  name: 'Microsoft.Default'
  properties: {
    mode: 'Blocking'
    contentFilters: [
      {
        name: 'Hate'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        name: 'Hate'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        name: 'Sexual'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        name: 'Sexual'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        name: 'Violence'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        name: 'Violence'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        name: 'Selfharm'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        name: 'Selfharm'
        severityThreshold: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
    ]
  }
}
*/

// resource aiFoundry_Microsoft_DefaultV2 'Microsoft.CognitiveServices/accounts/raiPolicies@2025-06-01' = {
//   parent: aiFoundry_resource
//   name: 'Microsoft.DefaultV2'
//   properties: {
//     mode: 'Blocking'
//     contentFilters: [
//       {
//         name: 'Hate'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Prompt'
//       }
//       {
//         name: 'Hate'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Completion'
//       }
//       {
//         name: 'Sexual'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Prompt'
//       }
//       {
//         name: 'Sexual'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Completion'
//       }
//       {
//         name: 'Violence'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Prompt'
//       }
//       {
//         name: 'Violence'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Completion'
//       }
//       {
//         name: 'Selfharm'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Prompt'
//       }
//       {
//         name: 'Selfharm'
//         severityThreshold: 'Medium'
//         blocking: true
//         enabled: true
//         source: 'Completion'
//       }
//       {
//         name: 'Jailbreak'
//         blocking: true
//         enabled: true
//         source: 'Prompt'
//       }
//       {
//         name: 'Protected Material Text'
//         blocking: true
//         enabled: true
//         source: 'Completion'
//       }
//       {
//         name: 'Protected Material Code'
//         blocking: false
//         enabled: true
//         source: 'Completion'
//       }
//     ]
//   }
// }

resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-04-01-preview' = {
  parent: aiFoundry_resource
  name: modelName
  sku: {
    name: skuName
    capacity: capacity
  }
  properties: {
    model: {
      format: modelPublisherFormat
      name: modelName
      version: modelVersion
    }
    raiPolicyName: contentFilterPolicyName == null ? 'Microsoft.Nill' : contentFilterPolicyName
  }
}

resource embeddingModelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-04-01-preview' = {
  parent: aiFoundry_resource
  name: embeddingModelName
  sku: {
    name: skuName
    capacity: capacity
  }
  properties: {
    model: {
      format: modelPublisherFormat
      name: embeddingModelName
    }
    raiPolicyName: contentFilterPolicyName == null ? 'Microsoft.Nill' : contentFilterPolicyName
  }
}
