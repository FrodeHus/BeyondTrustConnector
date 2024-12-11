namespace BeyondTrustConnector.Parser;

public class BeyondTrustLogEntry
{
    public DateTime Timestamp { get; set; }
    public int CorrelationId { get; set; }
    public Dictionary<string, string> Details { get; set; } = [];
}
