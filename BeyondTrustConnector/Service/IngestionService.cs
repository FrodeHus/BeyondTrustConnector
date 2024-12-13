using Azure.Identity;
using Azure.Monitor.Ingestion;
using BeyondTrustConnector.Model;
using BeyondTrustConnector.Parser;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector.Service;

public class IngestionService(ILogger<IngestionService> logger)
{
    private readonly ILogger<IngestionService> _logger = logger;

    public async Task IngestSyslog(List<BeyondTrustLogEntry> events)
    {
        await Ingest("BeyondTrustEvents_CL", events);
    }

    internal async Task IngestAccessSessions(List<BeyondTrustAccessSession> sessions)
    {
        await Ingest("BeyondTrustAccessSession_CL", sessions);
    }

    internal async Task IngestVaultActivity(List<BeyondTrustVaultActivity> vaultActivities)
    {
        await Ingest("BeyondTrustVaultActivity_CL", vaultActivities);
    }

    private async Task Ingest<TItem>(string tableName, List<TItem> items)
    {
        var dcrEndpoint = Environment.GetEnvironmentVariable("DCR_ENDPOINT") ?? throw new Exception("Variable not set: DCR_ENDPOINT");
        var dcrId = Environment.GetEnvironmentVariable("DCR_ID") ?? throw new Exception("Variable not set: DCR_ID");
        var endpoint = new Uri(dcrEndpoint);
        var creds = new DefaultAzureCredential();
        var client = new LogsIngestionClient(endpoint, creds);
        var response = await client.UploadAsync(dcrId, $"Custom-{tableName}", items);

        if (response.IsError)
        {
            _logger.LogError("Failed to upload data: {ErrorReason}", response.ReasonPhrase);
        }
        else
        {
            _logger.LogInformation("Uploaded {ItemCount} records to {TableName}", items.Count, tableName);
        }
    }
}
