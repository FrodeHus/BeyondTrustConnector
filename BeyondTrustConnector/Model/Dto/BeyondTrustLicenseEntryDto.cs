namespace BeyondTrustConnector.Model.Dto;

internal class BeyondTrustLicenseEntryDto
{
    public required string JumpItemName { get; set; }
    public required string HostnameOrIp { get; set; }
    public required string JumpMethod { get; set; }
    public required string JumpGroup { get; set; }
    public bool License { get; set; }
    public required string Jumpoint { get; set; }
    public string? RemoteApplicationName { get; set; }
}
