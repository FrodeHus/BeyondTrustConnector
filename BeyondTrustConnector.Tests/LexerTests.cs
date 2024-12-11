using BeyondTrustConnector.Parser;

namespace BeyondTrustConnector.Tests
{

    public class LexerTests
    {
        [Theory]
        [InlineData("Dec 10 07:04:11 tenant BG[12482]: 1427:01:01:event=login;site=tenant.beyondtrustcloud.com;status=success;target=web/login;when=1733810651;who=Test User (test@example.com) using saml;who_ip=12.3.4")]
        [InlineData("Dec 10 07:34:19 tenant BG[19414]: 1427:01:01:site=tenant.beyondtrustcloud.com;when=1733812459;who=API User (09415e9d78c193c8b2a5d5154e1102574beafdf1);who_ip=1.2.3.4;event=login;target=web/api;status=success")]
        [InlineData("Dec 11 08:50:51 tenant BG[69066]: 1427:05:05:y_name=Test User 2;old_permissions:jump_item_role:personal:id=1;old_permissions:jump_item_role:personal:name=No Access;old_public_display_name=Test User 3;old_rep_avatar=user photo deleted;old_permissions:jump_item_role:system:id=1;old_permissions:jump_item_role:system:name=No Access;old_external_id=@@@dGVzdC51c2VyQHRlbmFudC5jb20=")]
        public void ItCanLexBeyondTrustSyslogEvents(string logEntry)
        {
            var lexer = new SyslogLexer(logEntry);
            SyslogToken token;
            do
            {
                token = lexer.Lex();
                Assert.NotEqual(SyslogTokenKind.BadToken, token.Kind);
            } while (token.Kind != SyslogTokenKind.EndOfLogToken);
        }
    }
}
