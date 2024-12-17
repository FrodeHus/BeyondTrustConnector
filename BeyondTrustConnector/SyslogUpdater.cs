using System;
using BeyondTrustConnector.Parser;
using System.IO.Compression;
using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace BeyondTrustConnector
{
    public class SyslogUpdater(BeyondTrustService beyondTrustService, IngestionService ingestionService, QueryService queryService, ILogger<SyslogUpdater> logger)
    {
        [Function(nameof(SyslogUpdater))]
        public async Task Run([TimerTrigger("0 */15 * * * *", RunOnStartup = false)] TimerInfo myTimer)
        {
            DateTime? lastEventTime = await GetLastUpdatedTime();

            var syslogArchiveData = await beyondTrustService.DownloadReportAsync("Syslog");
            ZipArchive archive = new(new MemoryStream(syslogArchiveData));
            foreach (var entry in archive.Entries)
            {
                if (entry.Name.EndsWith("gz")) continue;

                logger.LogInformation("Reading {ZipEntry}", entry.Name);
                await using var entryStream = entry.Open();
                using var reportReader = new StreamReader(entryStream);
                var reportContent = await reportReader.ReadToEndAsync();
                var parser = new BeyondTrustLogParser(reportContent);
                var events = parser.Parse();
                if (lastEventTime.HasValue)
                {
                    events = events.Where(e => e.Timestamp > lastEventTime).ToList();
                }

                var filteredEvents = events.Where(e => e.AdditionalData.ContainsKey("who") && !e.AdditionalData["who"].StartsWith("Sentinel integration") && e.EventType != "syslog_report_generated");
                logger.LogInformation("Found {EventCount} total events - filtered to {FilteredEventCount}", events.Count, filteredEvents.Count());
                if (filteredEvents.Any())
                    await ingestionService.IngestSyslog(filteredEvents.ToList());
            }

        }

        private async Task<DateTime?> GetLastUpdatedTime()
        {
            var result = await queryService.QueryWorkspace("BeyondTrustEvents_CL | summarize arg_max(TimeGenerated,*) | project TimeGenerated");
            DateTime? lastEventTime = null;
            if (result?.Table.Rows.Count == 1 && result?.Table.Rows[0][0] is not null)
            {
                lastEventTime = ((DateTimeOffset)result.Table.Rows[0][0]).UtcDateTime;
            }
            logger.LogInformation("Last event time: {LastEventTime}", lastEventTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            return lastEventTime;
        }
    }
}
