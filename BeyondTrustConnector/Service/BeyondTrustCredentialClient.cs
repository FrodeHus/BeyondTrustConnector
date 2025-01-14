﻿using BeyondTrustConnector.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BeyondTrustConnector.Service;

internal class BeyondTrustCredentialClient(IHttpClientFactory httpClientFactory, ILogger<BeyondTrustCredentialClient> logger)
{
    public async Task<string> GetAccessToken(string beyondTrustUrl)
    {
        var secretName = Environment.GetEnvironmentVariable("KEYVAULT_SECRET") ?? throw new Exception("KEYVAULT_SECRET environment variable is not set");
        var secret = await SecretReader.GetSecretAsync(secretName);
        var credential = JsonSerializer.Deserialize<BeyondTrustCredential>(secret) ?? throw new Exception("Failed to deserialize BeyondTrust credential");
        var client = httpClientFactory.CreateClient();
        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credential.ClientId}:{credential.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuth);
        var response = await client.PostAsync($"{beyondTrustUrl}/oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            }));
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to get access token: {ErrorMessage}", message);
            throw new Exception("Failed to get access token");
        }
        var token = await response.Content.ReadFromJsonAsync<BeyondTrustAccessToken>();
        return token?.AccessToken ?? throw new Exception("Failed to get access token");
    }
}
