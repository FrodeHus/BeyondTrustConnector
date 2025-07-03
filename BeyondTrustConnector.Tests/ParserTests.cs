using BeyondTrustConnector.Parser;

namespace BeyondTrustConnector.Tests;

public class ParserTests
{

    [Fact]
    public void ItCanParseMultiSegmentLogs()
    {
        var log = @"Dec 11 08:50:51 tenant BG[69065]: 1427:01:05:site=tenant.beyondtrustcloud.com;when=1733903451;who=Test User (test.user@tenant.no);who_ip=45.90.36.3;event=user_changed;old_account:comments=;old_account:disabled=0;old_account:expiration=never;old_account:will_expire=0;old_provider:name=2;old_provider:id=Entra;old_username=test.user@tenant.no;old_account:created=2024-12-11 07:50:44.958881;old_permissions:jump_item_role:default:id=2;old_permissions:jump_item_role:default:name=Start Sessions Only;old_display_number=10;old_account:email:address=test.user@tenant.no;old_account:email:locale=en-us;old_account:failed_logins=0;old_id=62;old_login_schedule:timezone=UTC;old_permissions:jump_item_role:teams:id=1;old_permissions:jump_item_role:teams:name=No Access;old_last_notified=;old_license_pool:id=;old_license_pool:name=;old_two_factor_auth:required=0;old_login_sc
Dec 11 08:50:51 tenant BG[69065]: 1427:02:05:hedule:enabled=0;old_login_schedule:force_logout=0;old_two_factor_auth:app=two factor auth app not setup;old_password=***NEW***;old_password:expiration=;old_password:will_expire=0;old_password:reset=0;old_permissions:admin=0;old_permissions:support:jumpoint:admin=0;old_permissions:change_display_name=0;old_permissions:change_rep_image=0;old_permissions:access_sponsors=0;old_permissions:customer_notice=0;old_permissions:bomgar_button=0;old_permissions:canned_scripts=0;old_permissions:custom_rep_links=0;old_permissions:support:edit_ios_content=0;old_permissions:issues=0;old_permissions:jump_groups=0;old_permissions:support:canned_messages=0;old_permissions:file_store=0;old_permissions:public_sites:templates=0;old_permissions:teams=0;old_permissions:skills=0;old_permissions:presentations=0;old_permissions:presentations:control=0;old_permissions:support:accept_team_sessions=0;new_permissions
Dec 11 08:50:51 tenant BG[69065]: 1427:03:05::support:accept_team_sessions=1;old_permissions:support=full_support;old_permissions:support:bomgar_button:change_public_sites=0;old_permissions:support:bomgar_button:personal:deploy=0;old_permissions:support:bomgar_button:team:deploy=0;old_permissions:support:bomgar_button:team:manage=0;old_permissions:rep_to_rep_screen_sharing=1;old_permissions:rep_to_rep_screen_sharing:control=1;old_permissions:support:session_assignment:disable=0;old_permissions:support:external_key=0;old_permissions:support:extended_availability_mode=0;old_idle_timeout=site_wide_setting;old_permissions:support:session_keys=0;old_inactive_rep_timeout=site_wide_setting;old_permissions:support:invite_temp_rep=0;old_permissions:support:jumpoint:protocol_tunneling=0;old_permissions:support:next_session=0;old_permissions:support:jump:clients=1;old_permissions:support:jump:local=0;old_permissions:support:jump:remote=0;old_
Dec 11 08:50:51 tenant BG[69065]: 1427:04:05:permissions:support:rdp:local=0;old_permissions:support:rdp:remote=1;old_permissions:support:vnc:local=0;old_permissions:support:vnc:remote=1;old_permissions:support:session_assignment:idle_timeout=900;old_permissions:support:session_assignment:session_limit=3;old_permissions:support:team_share=0;old_permissions:support:jumpoint:shell=1;old_permissions:support:ios_content=0;old_permissions:support:vpro=0;old_permissions:support:jumpoint:website=1;old_permissions:support:team_transfer=0;old_permissions:users:set_passwords=0;old_permissions:show_on_public_site=0;old_permissions:vault=0;old_permissions:reporting:license_reports=0;old_permissions:reporting:presentation_reports=not_allowed;old_permissions:reporting:recordings=0;old_permissions:reporting:support_reports=not_allowed;old_permissions:reporting:syslog_reports=0;old_permissions:reporting:vault_reports=not_allowed;old_private_displa
Dec 11 08:50:51 tenant BG[69065]: 1427:05:05:y_name=Test User;old_permissions:jump_item_role:personal:id=1;old_permissions:jump_item_role:personal:name=No Access;old_public_display_name=Test User;old_rep_avatar=user photo deleted;old_permissions:jump_item_role:system:id=1;old_permissions:jump_item_role:system:name=No Access;old_external_id=@@@dGVzdC51c2VyQHRlbmFudC5ubw==
Dec 10 07:04:11 tenant BG[12482]: 1427:01:01:event=login;site=tenant.beyondtrustcloud.com;status=success;target=web/login;when=1733810651;who=Test User (test@example.com) using saml;who_ip=12.3.4
";
        var parser = new BeyondTrustLogParser(log, null);
        var events = parser.Parse();
        Assert.Equal(2, events.Count);
        Assert.Equal(101, events.First().AdditionalData.Count);
        Assert.Equal("", events.First().AdditionalData["old_account:comments"]);
    }

    [Fact]
    public void ItCanParseBeyondTrustLogEntries()
    {
        var log = @"Dec 10 07:04:11 tenant BG[12482]: 1427:01:01:event=login;site=tenant.beyondtrustcloud.com;status=success;target=web/login;when=1733810651;who=Test User (test@example.com) using saml;who_ip=12.3.4";
        var parser = new BeyondTrustLogParser(log, null);
        var events = parser.Parse();
        Assert.Single(events);
        Assert.Equal(5, events.First().AdditionalData.Count);
    }

    [Theory]
    [InlineData("event=login;",1)]
    [InlineData("event=login;site=tenant.beyondtrustcloud.com;",2)]
    [InlineData("event=login\\;test;site=tenant.beyondtrustcloud.com;",2)]
    [InlineData("event=login",1)]
    public void ItCanParsePayload(string payload, int expectedCount)
    {
        var parser = new PayloadParser(payload);
        var details = parser.Parse();
        Assert.Equal(expectedCount, details.Count());
    }
}
