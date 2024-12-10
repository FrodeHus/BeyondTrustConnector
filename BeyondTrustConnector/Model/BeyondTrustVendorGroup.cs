using System.Text.Json.Serialization;

namespace BeyondTrustConnector.Model;

public class BeyondTrustVendorGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    [JsonPropertyName("administrator_id")]
    public int AdministratorId { get; set; }
    [JsonPropertyName("default_policy")]
    public int DefaultPolicy { get; set; }
}


