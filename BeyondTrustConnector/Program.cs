using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddTransient<BeyondTrustCredentialClient>();
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
builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
