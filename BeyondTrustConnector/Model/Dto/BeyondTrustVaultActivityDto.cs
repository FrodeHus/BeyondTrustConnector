namespace BeyondTrustConnector.Model.Dto;

internal class BeyondTrustVaultActivityDto
{
    public DateTime Timestamp { get; set; }
    public required string EventType { get; set; }
    public string? SessionId { get; set; }
    public string? User { get; set; }
    public int UserId { get; set; }
    public int VaultAccountId { get; set; }
}
