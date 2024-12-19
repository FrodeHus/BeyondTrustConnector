using BeyondTrustConnector.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Xml.Linq;

namespace BeyondTrustConnector.Service
{
    public class BeyondTrustService(IHttpClientFactory httpClientFactory, ILogger<BeyondTrustService> logger)
    {
        public async Task<XDocument> GetAccessSessionReport(DateTime start, int reportPeriod = 0)
        {
            var client = httpClientFactory.CreateClient(nameof(BeyondTrustConnector));
            var unixTime = ((DateTimeOffset)start).ToUnixTimeSeconds();
            string requestUri = $"/api/reporting?generate_report=AccessSession&start_date={start:yyyy-MM-dd}&duration={reportPeriod}&start_time={unixTime}";
            var response = await client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to generate report: {ErrorMessage}", message);
                throw new Exception("Failed to generate report");
            }
            var reportContent = await response.Content.ReadAsStringAsync();
            
            return XDocument.Parse(reportContent);
        }

        public async Task<XDocument> GetVaultActivityReport(DateTime start, int reportPeriod = 0)
        {
            var client = httpClientFactory.CreateClient(nameof(BeyondTrustConnector));
            var unixTime = ((DateTimeOffset)start).ToUnixTimeSeconds() + 1;
            string requestUri = $"/api/reporting?generate_report=VaultAccountActivity&start_date={start:yyyy-MM-dd}&duration={reportPeriod}&start_time={unixTime}";
            var response = await client.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to generate report: {ErrorMessage}", message);
                throw new Exception("Failed to generate report");
            }
            var reportContent = await response.Content.ReadAsStringAsync();

            return XDocument.Parse(reportContent);
        }

        public async Task<byte[]> DownloadReportAsync(string report)
        {
            var client = httpClientFactory.CreateClient(nameof(BeyondTrustConnector));
            var response = await client.GetAsync($"/api/reporting?generate_report={report}");
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to download report: {ErrorMessage}", message);
                throw new Exception("Failed to download report");
            }
            var reportData = await response.Content.ReadAsByteArrayAsync();
            return reportData;
        }

        private async Task<TEntity> GetItem<TEntity>(string endpoint, string query)
        {
            var client = httpClientFactory.CreateClient(nameof(BeyondTrustConnector));
            var response = await client.GetAsync($"/api/config/v1/{endpoint}/{query}");
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to get configuration: {ErrorMessage}", message);
                throw new Exception("Failed to get configuration");
            }
            var item = await response.Content.ReadFromJsonAsync<TEntity>();
            return item is null ? throw new Exception("Failed to deserialize item") : item;
        }

        private async IAsyncEnumerable<TEntity> GetItems<TEntity>(string endpoint)
        {
            var client = httpClientFactory.CreateClient(nameof(BeyondTrustConnector));
            var response = await client.GetAsync($"/api/config/v1/{endpoint}");
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to get configuration: {ErrorMessage}", message);
                throw new Exception("Failed to get configuration");
            }
            var links = ParseLinkHeader(response);
            if (links.TryGetValue("next", out string? nextUrl))
            {
                var nextLink = nextUrl.Split('/').Last();
                var nextResponse = GetItems<TEntity>(nextLink);
                await foreach (var item in nextResponse)
                {
                    yield return item;
                }
            }
            var items = await response.Content.ReadFromJsonAsync<List<TEntity>>();
            if (items is null)
            {
                throw new Exception("Failed to deserialize items");
            }

            foreach (var item in items)
            {
                yield return item;
            }
        }
        public async Task<List<BeyondTrustVendorGroup>> GetVendorGroups()
        {
            var groups = new List<BeyondTrustVendorGroup>();
            await foreach (var item in GetItems<BeyondTrustVendorGroup>("vendor"))
            {
                groups.Add(item);
            }
            return groups;
        }

        public async Task<List<BeyondTrustUser>> GetUsers()
        {
            var users = new List<BeyondTrustUser>();
            await foreach (var item in GetItems<BeyondTrustUser>("user"))
            {
                users.Add(item);
            }
            return users;
        }

        public async Task<List<BeyondTrustUser>> GetVendorUsers(int vendorGroupId)
        {
            var vendorUsers = new List<BeyondTrustUser>();
            await foreach (var item in GetItems<BeyondTrustUser>($"vendor/{vendorGroupId}/user"))
            {
                vendorUsers.Add(item);
            }
            return vendorUsers;
        }

        public async Task<BeyondTrustUser> GetUser(string userId)
        {
            return await GetItem<BeyondTrustUser>("user", userId);
        }

        private Dictionary<string, string> ParseLinkHeader(HttpResponseMessage responseMessage)
        {
            var linkHeader = responseMessage.Headers.GetValues("Link").FirstOrDefault();
            var parsedLinks = new Dictionary<string, string>();
            if (linkHeader is not null)
            {
                var links = linkHeader.Split(',');
                foreach (var link in links)
                {
                    var parts = link.Split(';');
                    var url = parts[0].Trim('<', '>');
                    var rel = parts[1].Split('=')[1].Trim('"');
                    logger.LogInformation("Link: {Rel} - {Url}", rel, url);
                    parsedLinks[rel] = url;
                }
            }
            return parsedLinks;
        }
    }
}


