param workspaceName string

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
    name: workspaceName
}

resource vimBeyondTrustAuthentication  'Microsoft.OperationalInsights/workspaces/savedSearches@2023-09-01' = {
    parent: workspace
    name: 'vimAuthenticationBeyondTrust'
    properties: {
        category: 'Authentication'
        displayName: 'BeyondTrust - Authentication'
        functionAlias: 'vimAuthenticationBeyondTrust'
        functionParameters: 'starttime:datetime = datetime(null), endtime:datetime = datetime(null), targetusername_has:string = \'*\', disabled:bool = false'
        query: 'let BeyondTrustAuthParser=(\r\n    starttime: datetime=datetime(null),\r\n    endtime: datetime=datetime(null),\r\n    targetusername_has: string=\'*\',\r\n    disabled: bool=False\r\n    ) {\r\n    BeyondTrustEvents_CL\r\n    | where EventType in (\'login\', \'logout\')\r\n    | parse AdditionalData.who with UserDisplayName \' (\' UserAccountName \') using \' AuthMethod\r\n    | where (isnull(starttime) or TimeGenerated >= starttime)\r\n        and (isnull(endtime) or TimeGenerated <= endtime)\r\n        and (targetusername_has==\'*\' or (UserAccountName has targetusername_has))\r\n    | extend\r\n        EventType = iif(EventType == \'login\', \'Logon\', \'Logoff\'),\r\n        EventSchemaVersion = \'0.1.3\',\r\n        EventSchema = \'Authentication\',\r\n        EventProduct = \'Privileged Remote Access\',\r\n        EventVendor = \'BeyondTrust\',\r\n        EventCount = toint(1),\r\n        EventStartTime = TimeGenerated,\r\n        EventEndTime = TimeGenerated,\r\n        EventResult = iif(AdditionalData.status == \'success\', \'Success\', \'Failed\'),\r\n        Dvc = tostring(AdditionalData.site),\r\n        SrcIpAddress = iff(AdditionalData.who_ip != \'\', AdditionalData.who_ip, \'\')\r\n    | project-away AdditionalData, AuthMethod, CorrelationId, SiteId\r\n    | project-rename TargetUsername = UserAccountName\r\n    | project TimeGenerated, EventType, EventSchemaVersion, EventSchema, EventProduct, EventVendor, EventCount, EventStartTime, EventEndTime, EventResult, Dvc, TargetUsername, TargetDomain=Dvc,SrcIpAddress\r\n};\r\nBeyondTrustAuthParser(starttime=starttime, endtime=endtime, targetusername_has=targetusername_has, disabled=disabled)'
    }
}
