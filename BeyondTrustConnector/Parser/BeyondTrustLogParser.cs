
using BeyondTrustConnector.Model.Dto;

namespace BeyondTrustConnector.Parser;

internal class BeyondTrustLogParser(string log)
{
    public List<BeyondTrustLogEntryDto> Parse()
    {
        var entries = new List<BeyondTrustLogEntryDto>();
        var parser = new SyslogParser(log);
        foreach(var entry in parser.Entries)
        {
            var details = ParsePayload(entry.Payload);
            var beyondTrustLogEntry = new BeyondTrustLogEntryDto
            {
                Timestamp = UnixTimeStampToDateTimeUTC(int.Parse(details["when"])),
                CorrelationId = entry.CorrelationId,
                SiteId = entry.SiteId,
                SegmentId = entry.SegmentNumber,
                SegmentCount = entry.SegmentCount,
                EventType = details["event"],
                Hostname = entry.Hostname
            };
            details.Remove("event");
            details.Remove("when");
            beyondTrustLogEntry.AdditionalData =details;
            entries.Add(beyondTrustLogEntry);
        }
        return entries;
    }

    private static DateTime UnixTimeStampToDateTimeUTC(long unixTimeStamp)
    {
        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
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