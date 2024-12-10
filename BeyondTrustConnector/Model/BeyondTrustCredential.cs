using System.Text.Json.Serialization;

namespace BeyondTrustConnector.Model;

public class BeyondTrustCredential
{
    [JsonPropertyName("ClientID")]
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}


