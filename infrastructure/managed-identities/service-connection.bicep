param location string = resourceGroup().location
param userAssignedIdentities_wcl_ado_name string
param issuer string = 'https://login.microsoftonline.com/${tenant().tenantId}/v2.0'
param subject_identifier string

resource userAssignedIdentities_wcl_ado_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' = {
  name: userAssignedIdentities_wcl_ado_name
  location: location
}

resource userAssignedIdentities_wcl_ado_name_userAssignedIdentities_wcl_ado_name_credential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2025-01-31-preview' = {
  parent: userAssignedIdentities_wcl_ado_name_resource
  name: '${userAssignedIdentities_wcl_ado_name}-credential'
  properties: {
    issuer: issuer
    subject: subject_identifier
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}
