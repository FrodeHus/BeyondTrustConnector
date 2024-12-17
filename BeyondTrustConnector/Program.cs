using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BeyondTrustConnector.Tests")]
var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddTransient<BeyondTrustCredentialClient>();
builder.Services.AddTransient<QueryService>();
builder.Services.AddHttpClient(nameof(BeyondTrustConnector), (provider,client)=>
{
    var beyondTrustCredentialClient = provider.GetRequiredService<BeyondTrustCredentialClient>();
    var beyondTrustTenantName = Environment.GetEnvironmentVariable("BEYONDTRUST_TENANT") ?? throw new Exception("BEYONDTRUST_TENANT environment variable is not set");
    client.BaseAddress = new Uri($"https://{beyondTrustTenantName}.beyondtrustcloud.com");
    var token = beyondTrustCredentialClient.GetAccessToken(beyondTrustTenantName).Result;
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
});
builder.Services.AddTransient<IngestionService>();
builder.Services.AddTransient<BeyondTrustService>();
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Logging.AddApplicationInsights();
builder.ConfigureFunctionsWebApplication();


builder.Build().Run();
