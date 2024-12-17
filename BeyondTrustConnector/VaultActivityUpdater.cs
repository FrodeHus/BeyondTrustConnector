using System;
using System.Globalization;
using System.Xml.Linq;
using BeyondTrustConnector.Model;
using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector
{
    public class VaultActivityUpdater(BeyondTrustService beyondTrustService, QueryService queryService, IngestionService ingestionService, ILogger<VaultActivityUpdater> logger)
    {
        [Function(nameof(VaultActivityUpdater))]
        public async Task Run([TimerTrigger("0 */15 * * * *", RunOnStartup = false)] TimerInfo myTimer)
        {
            DateTime? lastEventTime = await GetLastUpdatedTime();
            lastEventTime ??= DateTime.Now.AddDays(-5);

            var ns = "http://www.beyondtrust.com/sra/namespaces/API/reporting";
            var report = await beyondTrustService.GetVaultActivityReport(lastEventTime.Value);
            var activities = report.Root!.Descendants(XName.Get("vault_account_activity", ns));
            var sessionBasedActivities = new string[] { "Credentials Used" };

            var vaultActivities = new List<BeyondTrustVaultActivity>();
            foreach (var activity in activities)
            {
                var timestamp = int.Parse(activity.Attribute("timestamp")!.Value);
                var eventType = activity.Attribute("event_type")!.Value;
                string? sessionId = null;
                if (sessionBasedActivities.Contains(eventType))
                {
                    sessionId = activity.Element(XName.Get("data", ns))!.Value;
                }

                var performedBy = activity.Element(XName.Get("performed_by", ns));
                int.TryParse(performedBy?.Attribute("id")?.Value, out var userId);
                var userName = performedBy?.Value;
                var vaultActivity = new BeyondTrustVaultActivity
                {
                    EventType = eventType,
                    Timestamp = UnixTimeStampToDateTimeUTC(timestamp),
                    SessionId = sessionId,
                    VaultAccountId = int.Parse(activity.Attribute("account")!.Value),
                    UserId = userId,
                    User = userName
                };
                vaultActivities.Add(vaultActivity);
            }

            await ingestionService.IngestVaultActivity(vaultActivities);
        }

        private static DateTime UnixTimeStampToDateTimeUTC(long unixTimeStamp)
        {
            DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }
        private async Task<DateTime?> GetLastUpdatedTime()
        {
            var result = await queryService.QueryWorkspace("BeyondTrustVaultActivity_CL | summarize arg_max(TimeGenerated,*) | project TimeGenerated");
            DateTime? lastEventTime = null;
            if (result?.Table.Rows.Count == 1 && result?.Table.Rows[0][0] is not null)
            {
                lastEventTime = ((DateTimeOffset)result.Table.Rows[0][0]).UtcDateTime;
            }
            if (lastEventTime is null)
                logger.LogWarning("No last updated timestamp found");
            else
                logger.LogInformation("Last event time: {LastEventTime}", lastEventTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            return lastEventTime;
        }
    }
}
