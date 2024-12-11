using System;
using BeyondTrustConnector.Parser;
using System.IO.Compression;
using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector
{
    public class SyslogUpdater(BeyondTrustService beyondTrustService, ILogger<SyslogUpdater> logger)
    {
        [Function(nameof(SyslogUpdater))]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            var syslogArchiveData = await beyondTrustService.DownloadReportAsync("Syslog");
            ZipArchive archive = new(new MemoryStream(syslogArchiveData));
            foreach (var entry in archive.Entries)
            {
                logger.LogInformation("Reading {ZipEntry}", entry.Name);
                await using var entryStream = entry.Open();
                using var reportReader = new StreamReader(entryStream);
                var reportContent = await reportReader.ReadToEndAsync();
                var parser = new BeyondTrustLogParser(reportContent);
                var events = parser.Parse();
                var filteredEvents = events.Where(e => e.Details.ContainsKey("who") && !e.Details["who"].StartsWith("Sentinel integration") && e.Details["event"] != "syslog_report_generated");
            }

        }
    }
}
