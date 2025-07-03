
using BeyondTrustConnector.Model.Dto;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector.Parser;

internal class BeyondTrustLogParser(string log, ILogger? logger)
{
    public List<BeyondTrustLogEntryDto> Parse()
    {
        var entries = new List<BeyondTrustLogEntryDto>();
        var parser = new SyslogParser(log);
        foreach(var entry in parser.Entries)
        {
            var details = ParsePayload(entry.Payload);
            if (!details.TryGetValue("when", out var when))
            {
                continue;
            }
            
            var beyondTrustLogEntry = new BeyondTrustLogEntryDto
            {
                Timestamp = UnixTimeStampToDateTimeUtc(int.Parse(when)),
                ProcessId = entry.ProcessId,
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

    private static DateTime UnixTimeStampToDateTimeUtc(long unixTimeStamp)
    {
        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }

    private Dictionary<string, string> ParsePayload(string payload)
    {
        var details = new Dictionary<string, string>();
        var payloadParser = new PayloadParser(payload);
        foreach (var (key, value) in payloadParser.Parse())
        {
            if (!details.TryAdd(key, value))
            {
                logger?.LogError($"Duplicate key: {key} - {value}");
            }
        }
        return details;
    }
}