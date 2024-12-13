namespace BeyondTrustConnector.Model;

public class BeyondTrustAccessSession
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
}
