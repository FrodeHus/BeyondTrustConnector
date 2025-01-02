using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BeyondTrustConnector.Parser;

internal partial class SyslogParser
{
    [GeneratedRegex(@"(?<month>[A-Za-z]{3})\s{1,2}(?<timestamp>\d{1,2}\s\d{2}:\d{2}:\d{2})\s(?<hostname>[a-z]+)\sBG\[(?<correlationId>\d{1,7})\]:\s(?<siteId>\d{1,5}):(?<segmentNumber>\d{2}):(?<segmentCount>\d{2}):(?<payload>.+)", RegexOptions.Compiled)]
    private static partial Regex SyslogRegex();
    internal List<SyslogEntry> Entries { get; } = [];
    internal SyslogParser(string log)
    {
        using var reader = new StringReader(log) ?? throw new ArgumentNullException(nameof(log));
        string? line;
        SyslogEntry? entry = null;
        while ((line = reader.ReadLine()) != null)
        {
            if (entry != null && entry.SegmentNumber != entry.SegmentCount)
            {
                entry = Parse(line, entry);
            }
            else
            {
                entry = Parse(line);
            }

            if (entry != null && entry.SegmentNumber == entry.SegmentCount)
            {
                Entries.Add(entry);
            }
        }

    }

    private static SyslogEntry? Parse(string line, SyslogEntry? entry = null)
    {
        if (line == null)
        {
            return null;
        }

        var match = SyslogRegex().Match(line);
        if (!match.Success)
        {
            return null;
        }

        var month = match.Groups["month"].Value;
        var timestamp = match.Groups["timestamp"].Value;
        var hostname = match.Groups["hostname"].Value;
        var correlationId = int.Parse(match.Groups["correlationId"].Value);
        var siteId = int.Parse(match.Groups["siteId"].Value);
        var segmentNumber = int.Parse(match.Groups["segmentNumber"].Value);
        var segmentCount = int.Parse(match.Groups["segmentCount"].Value);
        var payload = match.Groups["payload"].Value;
        if (entry != null)
        {
            entry.Payload += payload;
            entry.SegmentNumber = segmentNumber;
            return entry;
        }

        entry = new SyslogEntry
        {
            Hostname = hostname,
            CorrelationId = correlationId,
            SiteId = siteId,
            SegmentNumber = segmentNumber,
            SegmentCount = segmentCount,
            Payload = payload
        };
        return entry;
    }

    internal class SyslogEntry
    {
        public required string Hostname { get; set; }
        public int CorrelationId { get; set; }
        public int SiteId { get; set; }
        public int SegmentNumber { get; set; }
        public int SegmentCount { get; set; }
        public required string Payload { get; set; }
    }
}