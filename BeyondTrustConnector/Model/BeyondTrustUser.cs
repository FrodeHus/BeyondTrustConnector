using System.Text.Json.Serialization;

namespace BeyondTrustConnector.Model;

public class BeyondTrustUser
{
    public int Id { get; set; }
    [JsonPropertyName("public_display_name")]
    public required string Name { get; set; }
    [JsonPropertyName("username")]
    public required string UserName { get; set; }
    [JsonPropertyName("email_address")]
    public required string Email { get; set; }
    [JsonPropertyName("is_expired")]
    public bool IsExpired { get; set; }
    [JsonPropertyName("vendor_administrator")]
    public bool IsVendorAdministrator { get; set; }
    [JsonPropertyName("security_provider_id")]
    public int SecurityProviderId { get; set; }
    [JsonPropertyName("last_authentication")]
    public DateTime? LastAuthentication { get; set; }
    [JsonPropertyName("two_factor_required")]
    public bool MfaRequired { get; set; }
    [JsonPropertyName("two_factor_enabled")]
    public bool MfaEnabled { get; set; }
}


