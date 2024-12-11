using Azure.Identity;
using Azure.Monitor.Ingestion;
using BeyondTrustConnector.Model;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector.Service;

public class IngestionService(ILogger<IngestionService> logger)
{
    private readonly ILogger<IngestionService> _logger = logger;

    public async Task IngestSyslog(List<string> events)
    {
        await Ingest("Syslog", events);
    }


    private async Task Ingest<TItem>(string tableName, List<TItem> items)
    {
        var endpoint = new Uri($"https://ve-secops-endpoint-13g1.northeurope-1.ingest.monitor.azure.com");
        var creds = new DefaultAzureCredential();
        var client = new LogsIngestionClient(endpoint, creds);
        var response = await client.UploadAsync("dcr-d70c743ae56c43c2ae137477830260ad", $"Custom-{tableName}", items);

        if (response.IsError)
        {
            _logger.LogError($"Failed to upload data: {response.ReasonPhrase}");
        }
        else
        {
            _logger.LogInformation($"Uploaded {items.Count} records");
        }
    }
}
