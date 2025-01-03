param workspaceName string

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: workspaceName
}

resource workbookId_resource 'microsoft.insights/workbooks@2022-04-01' = {
  name: guid('BeyondTrustSigninsWorkbook')
  location: resourceGroup().location
  kind: 'shared'
  properties: {
    displayName: 'BeyondTrust SignIns'
    serializedData: '{"version":"Notebook/1.0","items":[{"type":1,"content":{"json":"## BeyondTrust\\n---\\n"},"name":"text - 2"},{"type":3,"content":{"version":"KqlItem/1.0","query":"vimAuthenticationBeyondTrust\\n| make-series Users=dcount(TargetUsername) default = 0 on TimeGenerated from ago(7d) to now() step 1d\\n| render barchart ","size":1,"aggregation":5,"title":"Usage","queryType":0,"resourceType":"microsoft.operationalinsights/workspaces"},"name":"query - 2"},{"type":3,"content":{"version":"KqlItem/1.0","query":"vimAuthenticationBeyondTrust\\r\\n| order by TimeGenerated desc","size":0,"title":"Sign ins","timeContext":{"durationMs":86400000},"queryType":0,"resourceType":"microsoft.operationalinsights/workspaces","gridSettings":{"formatters":[{"columnMatch":"TimeGenerated","formatter":7,"formatOptions":{"linkTarget":"GenericDetails","linkIsContextBlade":true}},{"columnMatch":"EventSchemaVersion","formatter":5},{"columnMatch":"EventSchema","formatter":5},{"columnMatch":"EventCount","formatter":5},{"columnMatch":"EventStartTime","formatter":5},{"columnMatch":"EventEndTime","formatter":5},{"columnMatch":"EventProduct","formatter":5},{"columnMatch":"EventVendor","formatter":5},{"columnMatch":"Dvc","formatter":5},{"columnMatch":"TargetDomain","formatter":5},{"columnMatch":"UserAccountName","formatter":7,"formatOptions":{"linkTarget":"GenericDetails","linkIsContextBlade":true}},{"columnMatch":"UserDisplayName","formatter":5}]}},"customWidth":"50","name":"query - 2"},{"type":3,"content":{"version":"KqlItem/1.0","query":"vimAuditBeyondTrust\\r\\n| order by TimeGenerated desc","size":0,"title":"Audit","timeContext":{"durationMs":604800000},"queryType":0,"resourceType":"microsoft.operationalinsights/workspaces","gridSettings":{"formatters":[{"columnMatch":"TimeGenerated","formatter":7,"formatOptions":{"linkTarget":"GenericDetails","linkIsContextBlade":true}},{"columnMatch":"EventSchemaVersion","formatter":5},{"columnMatch":"EventSchema","formatter":5},{"columnMatch":"EventCount","formatter":5},{"columnMatch":"EventStartTime","formatter":5},{"columnMatch":"EventEndTime","formatter":5},{"columnMatch":"EventProduct","formatter":5},{"columnMatch":"EventVendor","formatter":5},{"columnMatch":"Dvc","formatter":5},{"columnMatch":"ObjectType","formatter":5}]}},"customWidth":"50","name":"query - 3"},{"type":3,"content":{"version":"KqlItem/1.0","query":"imBeyondTrustAccessSessions\\r\\n| order by TimeGenerated desc\\r\\n| take 20","size":0,"title":"Access Sessions","timeContext":{"durationMs":86400000},"queryType":0,"resourceType":"microsoft.operationalinsights/workspaces","gridSettings":{"formatters":[{"columnMatch":"EndTime","formatter":5},{"columnMatch":"JumpItemAddress","formatter":7,"formatOptions":{"linkTarget":"GenericDetails","linkIsContextBlade":true}},{"columnMatch":"JumpItemId","formatter":5},{"columnMatch":"SessionId","formatter":5},{"columnMatch":"SessionType","formatter":5},{"columnMatch":"UserDetails","formatter":5},{"columnMatch":"TenantId","formatter":5},{"columnMatch":"Type","formatter":5},{"columnMatch":"_ResourceId","formatter":5},{"columnMatch":"ClientPublicIp","formatter":5},{"columnMatch":"ClientPrivateIp","formatter":5},{"columnMatch":"ClientDevice","formatter":5},{"columnMatch":"UserCount","formatter":5},{"columnMatch":"ChatDownloadUrl","formatter":5},{"columnMatch":"ChatViewUrl","formatter":5},{"columnMatch":"FileTransferCount","formatter":5},{"columnMatch":"FileDeleteCount","formatter":5},{"columnMatch":"FileMoveCount","formatter":5}]}},"name":"query - 4"}],"isLocked":false,"fallbackResourceIds":["${workspace.id}"],"fromTemplateId":"sentinel-UserWorkbook"}'
    version: '1.0'
    sourceId: workspace.id
    category: 'sentinel'
  }
  dependsOn: []
}

output workbookId string = workbookId_resource.id
