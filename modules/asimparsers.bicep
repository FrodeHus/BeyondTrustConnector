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
        query: 'let BeyondTrustAuthParser=(\r\n    starttime: datetime=datetime(null),\r\n    endtime: datetime=datetime(null),\r\n    targetusername_has: string=\'*\',\r\n    disabled: bool=False\r\n    ) {\r\n    BeyondTrustEvents_CL\r\n    | where EventType in (\'login\', \'logout\')\r\n    | parse AdditionalData.who with UserDisplayName \' (\' UserAccountName \') using \' AuthMethod\r\n    | where (isnull(starttime) or TimeGenerated >= starttime)\r\n        and (isnull(endtime) or TimeGenerated <= endtime)\r\n        and (targetusername_has==\'*\' or (UserAccountName has targetusername_has))\r\n    | extend\r\n        EventType = iif(EventType == \'login\', \'Logon\', \'Logoff\'),\r\n        EventSchemaVersion = \'0.1.3\',\r\n        EventSchema = \'Authentication\',\r\n        EventProduct = \'Privileged Remote Access\',\r\n        EventVendor = \'BeyondTrust\',\r\n        EventCount = toint(1),\r\n        EventStartTime = TimeGenerated,\r\n        EventEndTime = TimeGenerated,\r\n        EventResult = iif(AdditionalData.status == \'failure\', \'Failure\', \'Success\'),\r\n        Dvc = tostring(AdditionalData.site),\r\n        SrcIpAddress = iff(AdditionalData.who_ip != \'\', AdditionalData.who_ip, \'\')\r\n    | project-away AdditionalData, AuthMethod, CorrelationId, SiteId\r\n    | project-rename TargetUsername = UserAccountName\r\n    | project TimeGenerated, EventType, EventSchemaVersion, EventSchema, EventProduct, EventVendor, EventCount, EventStartTime, EventEndTime, EventResult, Dvc, TargetUsername, TargetDomain=Dvc,SrcIpAddress\r\n};\r\nBeyondTrustAuthParser(starttime=starttime, endtime=endtime, targetusername_has=targetusername_has, disabled=disabled)'
    }
}

resource vimBeyondTrustAudit  'Microsoft.OperationalInsights/workspaces/savedSearches@2023-09-01' = {
    parent: workspace
    name: 'vimAuditBeyondTrust'
    properties: {
        category: 'Audit'
        displayName: 'BeyondTrust - Audit'
        functionAlias: 'vimAuditBeyondTrust'
        functionParameters: 'starttime:datetime = datetime(null), endtime:datetime = datetime(null), srcipaddr_has_any_prefix:dynamic = dynamic(null), eventtype_in:string = \'*\', operation_has_any:dynamic = dynamic(null), object_has_any:dynamic = dynamic(null), newvalue_has_any:dynamic = dynamic(null), disabled:bool = False, eventresult:string = \'*\''
        query: 'let BeyondTrustAuditParser = (\r\n    starttime: datetime=datetime(null),\r\n    endtime: datetime=datetime(null),\r\n    srcipaddr_has_any_prefix: dynamic=dynamic(null),\r\n    eventtype_in: string = \'*\',\r\n    eventresult: string = \'*\',\r\n    actorusername_has_any: dynamic = dynamic(null),\r\n    operation_has_any: dynamic = dynamic(null),\r\n    object_has_any: dynamic = dynamic(null),\r\n    newvalue_has_any: dynamic = dynamic(null),\r\n    disabled: bool=False\r\n) {\r\nBeyondTrustEvents_CL\r\n| where EventType in (\'user_changed\',\'api_account_changed\')\r\n| extend ActorUsername = iif(AdditionalData.who != \'\', AdditionalData.who, \'\')\r\n| extend Object = iif(AdditionalData.old_username != \'\', AdditionalData.old_username, iif(AdditionalData.old_client_id != \'\', AdditionalData.old_client_id, \'\'))\r\n| where (isnull(starttime) or TimeGenerated >= starttime)\r\n        and (isnull(endtime) or TimeGenerated <= endtime)\r\n        and (isnull(object_has_any) or (Object has_any (object_has_any)))        \r\n| extend\r\n        EventType = iif(EventType contains \'changed\', \'Set\', \'Other\'),\r\n        EventSchemaVersion = \'0.1\',\r\n        EventSchema = \'AuditEvent\',\r\n        EventProduct = \'Privileged Remote Access\',\r\n        EventVendor = \'BeyondTrust\',\r\n        EventCount = toint(1),\r\n        EventStartTime = TimeGenerated,\r\n        EventEndTime = TimeGenerated,\r\n        EventResult = \'Success\',\r\n        Dvc = tostring(AdditionalData.site),\r\n        Operation = \'UserChanged\',\r\n        TargetAppType = \'SaaS application\',\r\n        TargetAppName = \'BeyondTrust PRA\',\r\n        SrcIpAddress = iff(AdditionalData.who_ip != \'\', AdditionalData.who_ip, \'\'),\r\n   ObjectType = \'Other\'\r\n| mv-apply key=bag_keys(AdditionalData) on (\r\n    where key startswith \'new_\'\r\n    | summarize NewValue=make_list(pack(\'Name\',substring(key,4), \'Value\',tostring(AdditionalData[tostring(key)])), 100)\r\n    )\r\n|mv-apply key=bag_keys(AdditionalData) on (\r\n    where key startswith \'old_\'\r\n and NewValue has substring(key,4)   | summarize OldValue=make_list(pack(\'Name\',substring(key,4), \'Value\',tostring(AdditionalData[tostring(key)])), 100)\r\n    )\r\n| where (isnull(operation_has_any) or (Operation has_any (object_has_any)))\r\n    and (isnull(newvalue_has_any) or (NewValue  has_any (newvalue_has_any)))\r\n    and (isnull(srcipaddr_has_any_prefix) or (SrcIpAddress  has_any (srcipaddr_has_any_prefix)))\r\n    and (eventresult == \'*\' or EventResult == eventresult)\r\n    and (eventtype_in == \'*\' or EventType in (eventtype_in))\r\n| project-away AdditionalData, CorrelationId, SiteId, Hostname\r\n| project TimeGenerated, EventType, EventSchemaVersion, EventSchema, EventProduct, EventVendor, EventCount, EventStartTime, EventEndTime, EventResult, Dvc, ActorUsername, Object, ObjectType, TargetAppName, TargetAppType, OldValue, NewValue,SrcIpAddress\r\n};\r\nBeyondTrustAuditParser(starttime=starttime, endtime=endtime,srcipaddr_has_any_prefix=srcipaddr_has_any_prefix, eventtype_in=eventtype_in, newvalue_has_any=newvalue_has_any, operation_has_any=operation_has_any, object_has_any=object_has_any, disabled=disabled)'
    }
}

resource imBeyondTrustAccessSessions  'Microsoft.OperationalInsights/workspaces/savedSearches@2023-09-01' = {
    parent: workspace
    name: 'imBeyondTrustAccessSessions'
    properties: {
        category: 'Audit'
        displayName: 'BeyondTrust - Session Audit'
        functionAlias: 'imBeyondTrustAccessSessions'
        query: 'BeyondTrustAccessSession_CL\r\n| where not(isempty(SessionId))\r\n| mv-apply detail=UserDetails on (\r\n\twhere detail.SessionOwner == true\r\n\t| extend Username=detail.Username\r\n\t| extend ClientPublicIp = detail.PublicIP\r\n\t| extend ClientPrivateIp = detail.PrivateIP\r\n\t| extend ClientDevice = detail.Hostname\r\n)\r\n| extend UserCount=array_length(UserDetails)'
    }
}