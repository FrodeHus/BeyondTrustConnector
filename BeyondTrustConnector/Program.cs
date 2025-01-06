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
    var beyondTrustUrl = Environment.GetEnvironmentVariable("BEYONDTRUST_URL") ?? throw new Exception("BEYONDTRUST_URL environment variable is not set");
    client.BaseAddress = new Uri(beyondTrustUrl);
    var token = beyondTrustCredentialClient.GetAccessToken(beyondTrustUrl).Result;
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
});
builder.Services.AddTransient<IngestionService>();
builder.Services.AddTransient<BeyondTrustService>();
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Logging.AddApplicationInsights();
builder.ConfigureFunctionsWebApplication();


builder.Build().Run();
