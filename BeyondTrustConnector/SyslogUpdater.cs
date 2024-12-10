using System;
using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector
{
    public class SyslogUpdater(BeyondTrustService beyondTrustService, ILogger<SyslogUpdater> logger)
    {
        [Function(nameof(SyslogUpdater))]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            var users = await beyondTrustService.GetUsers();
        }
    }
}
