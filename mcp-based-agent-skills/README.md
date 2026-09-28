# MCP-based agent skills

Based on sample code from the article [Discover Agent Skills from MCP servers in .NET | Microsoft Agent Framework](https://devblogs.microsoft.com/agent-framework/discover-agent-skills-from-mcp-servers-in-net/) - original source  [Agent_Step06_McpBasedSkills at main · microsoft/agent-framework](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/AgentSkills/Agent_Step06_McpBasedSkills).

The code needs an Azure Open AI endpoint with a deployed model. The following need to be set as environment variables:
```
$env:AZURE_OPENAI_ENDPOINT="https://your-endpoint.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_NAME="gpt-5.4-mini"
$env:AZURE_OPENAI_API_KEY="your-api-key"
```

or user secrets (this takes precedence over environment variables):
```
{
  "AZURE_OPENAI_ENDPOINT": "https://your-endpoint.openai.azure.com/",
  "AZURE_OPENAI_DEPLOYMENT_NAME": "gpt-5.4-mini",
  "AZURE_OPENAI_API_KEY": "your-api-key"
}
```

If using the Azure OpenAI client directly with the API key, the endpoint should be of the form
```
  "AZURE_OPENAI_ENDPOINT": "https://your-foundry=account-name.services.ai.azure.com/openai/v1",
```

## Troubleshooting permissions

If using the Foundry project client (which uses a `DefaultAzureCredential`), make sure your account has the required permissions.
```
$subscriptionId = az account show --query id -o tsv
$resourceGroup = "<resource-group>"
$assignee = "<user-email>"
$accountName = "mw-ai-models"
$projectName = "mw-foundry-eastus2"

$projectScope = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.CognitiveServices/accounts/$accountName/projects/$projectName"
$accountScope = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.CognitiveServices/accounts/$accountName"

$principalId = az ad user show `
  --id $assignee `
  --query id `
  --output tsv

az role assignment create `
  --assignee $assignee `
  --role "Azure AI User" `
  --scope $projectScope

# If Azure AI User does not exist use
az role assignment create `
  --assignee $assignee `
  --role "Azure AI Developer" `
  --scope $projectScope

# If Azure AI Developer isn't assigned here, use the principalId instead of the assignee email address
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type User `
  --role "Azure AI Developer" `
  --scope $projectScope

# If the project scope is rejected, assign the role at the parent Foundry account scope instead:
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type User `
  --role "Azure AI Developer" `
  --scope $accountScope

az role assignment create `
  --assignee $assignee `
  --role "Cognitive Services OpenAI User" `
  --scope $accountScope```
```

To list available role assignments (e.g. to check if Azure AI User exists) use
```
az role definition list `
  --query "[?contains(roleName, 'AI') || contains(roleName, 'Cognitive Services')].{Name:roleName,Id:name}" `
  --output table
```

Verify assignments:
```
az role assignment list `
  --assignee $assignee `
  --scope $accountScope `
  --include-inherited `
  --query "[].{role:roleDefinitionName,scope:scope}" `
  --output table
```

Other useful commands:

Get assigned user
```
az ad signed-in-user show `
  --query "{ObjectId:id,UserPrincipalName:userPrincipalName}" `
  --output table
```

Check that the assigned role actually contains the required data action:
```
az role definition list `
  --name "Cognitive Services OpenAI User" `
  --query "[0].dataActions" `
  --output json
```

If permissions are still insufficient, assign the "Cognitive Services OpenAI Contributor" role instead:
```
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type User `
  --role "Cognitive Services OpenAI Contributor" `
  --scope $accountScope
```

If this role exists:
```
az role definition list `
  --name "Azure AI Inference Deployment Operator" `
  --query "[].{Name:roleName,Id:name,Permissions:permissions}" `
  --output json
```

Add it
```
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type User `
  --role "Azure AI Inference Deployment Operator" `
  --scope $projectScope
```

Also assign Cognitive Services OpenAI User directly at the project scope, in addition to the existing account-level assignment:
```
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type User `
  --role "Cognitive Services OpenAI User" `
  --scope $projectScope
```

Assign Cognitive Services OpenAI User and Azure AI Inference Deployment Operator at the account scope as well:
```
az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type User `
  --role "Cognitive Services OpenAI User" `
  --scope $projectScope

az role assignment create `
  --assignee-object-id $principalId `
  --assignee-principal-type User `
  --role "Azure AI Inference Deployment Operator" `
  --scope $projectScope
```

You might also need "Foundry User" at the project scope. This is untested but should work:
```
az role assignment create `
  --assignee $assignee `
  --role "Foundry User"" `
  --scope $projectScope
```

For reference, the following worked in a different project (untested here and might need new variables set up):
```
$projectId = az cognitiveservices account project show `
  --name $foundryResourceName `
  --resource-group $resourceGroup `
  --project-name $projectName `
  --query id `
  -o tsv

$userObjectId = az ad signed-in-user show --query id -o tsv

az role assignment create `
  --assignee $userObjectId `
  --role "Foundry User" `
  --scope $projectId
```

Verify the effective project assignments:
```
az role assignment list `
  --assignee-object-id $principalId `
  --scope $projectScope `
  --include-inherited `
  --query "[].{Role:roleDefinitionName,Scope:scope,Condition:condition}" `
  --output table
```