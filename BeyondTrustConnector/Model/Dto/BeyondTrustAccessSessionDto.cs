namespace BeyondTrustConnector.Model.Dto;

public class BeyondTrustAccessSessionDto
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public required string SessionId { get; set; }
    public required string Jumpoint { get; set; }
    public required string SessionType { get; set; }
    public required string JumpGroup { get; set; }
    public required string JumpItemAddress { get; set; }
    public int JumpItemId { get; set; }
    public IEnumerable<Dictionary<string, object>> UserDetails { get; set; } = [];
    public int? FileTransferCount { get; set; }
    public int? FileMoveCount { get; set; }
    public int? FileDeleteCount { get; set; }
    public IEnumerable<Dictionary<string, object>> Events { get; internal set; } = [];
    public string? ChatDownloadUrl { get; internal set; }
    public string? ChatViewUrl { get; internal set; }
    public int JumpGroupId { get; internal set; }
}
