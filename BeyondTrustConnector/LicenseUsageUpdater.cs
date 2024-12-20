using System;
using System.IO.Compression;
using BeyondTrustConnector.Model.Dto;
using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector
{
    public class LicenseUsageUpdater(BeyondTrustService beyondTrustService, IngestionService ingestionService, ILogger<LicenseUsageUpdater> logger)
    {
        [Function(nameof(LicenseUsageUpdater))]
        public async Task Run([TimerTrigger("0 0 8 * * *", RunOnStartup = false)] TimerInfo myTimer)
        {
            var reportArchive = await beyondTrustService.GetEndpointLicenseUsageReportAsync();
            ZipArchive archive = new(new MemoryStream(reportArchive));
            var jumpItemReport = archive.GetEntry("jump_items.csv");
            if (jumpItemReport is null)
            {
                logger.LogError("Jump Items report not found in archive");
                throw new Exception("Jump Items report not found in archive");
            }
            
            using var jumpItemStream = jumpItemReport.Open();
            using var jumpItemReader = new StreamReader(jumpItemStream);
            var csvLine = await jumpItemReader.ReadLineAsync();

            var licenseEntries = new List<BeyondTrustLicenseEntryDto>();
            while ((csvLine = await jumpItemReader.ReadLineAsync()) != null){
                var fields = csvLine.Split(',');
                var licenseEntry = new BeyondTrustLicenseEntryDto
                {
                    JumpItemName = fields[0].Trim('\"'),
                    HostnameOrIp = fields[1].Trim('\"'),
                    Jumpoint = fields[2].Trim('\"'),
                    RemoteApplicationName = fields[3].Trim('\"'),
                    License = fields[4].Trim('\"') == "yes",
                    JumpMethod = fields[5].Trim('\"'),
                    JumpGroup = fields[6].Trim('\"'),
                };
                licenseEntries.Add(licenseEntry);
            }
            if(licenseEntries.Count == 0)
            {
                logger.LogWarning("No license entries found in report");
                return;
            }

            await ingestionService.IngestLicenseUsage(licenseEntries);
        }
    }
}
