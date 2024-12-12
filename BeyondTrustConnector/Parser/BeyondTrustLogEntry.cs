namespace BeyondTrustConnector.Parser;

public class BeyondTrustLogEntry
{
    public DateTime Timestamp { get; set; }
    public int CorrelationId { get; set; }
    public int SiteId { get; set; }
    public int SegmentId { get; set; }
    public int SegmentCount { get; set; }
    public Dictionary<string, string> Details { get; set; } = [];
}
