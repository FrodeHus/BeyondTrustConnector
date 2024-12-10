using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace BeyondTrustConnector.Service
{
    internal class SecretReader
    {
        public static async Task<string> GetSecretAsync(string secretName)
        {
            var keyvaultName = Environment.GetEnvironmentVariable("KEYVAULT_NAME") ?? throw new Exception("KEYVAULT_NAME environment variable is not set");

            SecretClientOptions options = new()
            {
                Retry =
                        {
                            Delay= TimeSpan.FromSeconds(2),
                            MaxDelay = TimeSpan.FromSeconds(16),
                            MaxRetries = 5,
                            Mode = RetryMode.Exponential
                         }
            };
            var client = new SecretClient(new Uri($"https://{keyvaultName}.vault.azure.net/"), new DefaultAzureCredential(includeInteractiveCredentials: true), options);
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}
