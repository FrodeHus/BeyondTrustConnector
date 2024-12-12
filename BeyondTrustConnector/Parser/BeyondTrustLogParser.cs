
namespace BeyondTrustConnector.Parser;

internal class BeyondTrustLogParser(string log)
{
    public List<BeyondTrustLogEntry> Parse()
    {
        var entries = new List<BeyondTrustLogEntry>();
        var parser = new SyslogParser(log);
        foreach(var entry in parser.Entries)
        {
            var beyondTrustLogEntry = new BeyondTrustLogEntry
            {
                Timestamp = entry.Timestamp,
                CorrelationId = entry.CorrelationId,
                SiteId = entry.SiteId,
                SegmentId = entry.SegmentNumber,
                SegmentCount = entry.SegmentCount
            };
            beyondTrustLogEntry.Details = ParsePayload(entry.Payload);
            entries.Add(beyondTrustLogEntry);
        }
        return entries;
    }

    private static Dictionary<string, string> ParsePayload(string payload)
    {
        var details = new Dictionary<string, string>();
        var payloadParser = new PayloadParser(payload);
        foreach (var (Key, Value) in payloadParser.Parse())
        {
            details.Add(Key, Value);
        }
        return details;
    }
}