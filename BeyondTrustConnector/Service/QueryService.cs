using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query.Models;
using Azure.Monitor.Query;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BeyondTrustConnector.Service;

public class QueryService(ILogger<QueryService> logger, IHttpClientFactory httpClientFactory)
{
    private readonly ILogger<QueryService> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<string> AdvancedQueryAsync(string query)
    {
        var creds = new DefaultAzureCredential();
        var token = await creds.GetTokenAsync(new TokenRequestContext(["https://api.securitycenter.microsoft.com/.default"]));
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri("https://api.securitycenter.microsoft.com");
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
        var queryPayload = JsonContent.Create(new AdvancedQuery { Query = query });
        var response = await httpClient.PostAsync("/api/advancedqueries/run", queryPayload);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Query failed: {message}");
            return "{}";
        }
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<LogsQueryResult?> QueryWorkspace(string query)
    {
        var principalId = Environment.GetEnvironmentVariable("PRINCIPAL_ID") ?? throw new Exception("PRINCIPAL_ID environment variable is not set");
        var client = new LogsQueryClient(new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = principalId}));
        var workspaceId = Environment.GetEnvironmentVariable("WORKSPACE_ID");
        var response = await client.QueryWorkspaceAsync(workspaceId, query, new QueryTimeRange(TimeSpan.FromDays(1)));
        if (response.Value.Status != LogsQueryResultStatus.Success)
        {
            _logger.LogError("Query failed: {Error}", response.Value.Error.Message);
            return null;
        }
        return response.Value;
    }

    private class AdvancedQuery
    {
        [JsonPropertyName("Query")]
        public required string Query { get; set; }
    }
}
