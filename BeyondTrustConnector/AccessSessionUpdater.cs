using System.Globalization;
using System.Xml.Linq;
using BeyondTrustConnector.Model.Dto;
using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector;

public class AccessSessionUpdater(BeyondTrustService beyondTrustService, IngestionService ingestionService, QueryService queryService, ILogger<AccessSessionUpdater> logger)
{
    [Function(nameof(AccessSessionUpdater))]
    public async Task Run([TimerTrigger("0 */15 * * * *", RunOnStartup = false)] TimerInfo myTimer)
    {
        DateTime? lastEventTime = await GetLastUpdatedTime();
        lastEventTime ??= DateTime.Now.AddDays(-5);

        var report = await beyondTrustService.GetAccessSessionReport(lastEventTime.Value);
        var sessions = report.Items.Select(i => i.ToDto()).ToList();
        var existingSessions = await CheckIfSessionsAlreadyExists(queryService, sessions);

        var accessSessions = sessions.Where(s => !existingSessions.Contains(s.SessionId)).ToList();

        if (accessSessions.Count != 0)
        {
            await ingestionService.IngestAccessSessions(accessSessions);
        }
    }

    private static async Task<List<string?>> CheckIfSessionsAlreadyExists(QueryService queryService, IEnumerable<BeyondTrustAccessSessionDto> sessions)
    {
        var sessionIds = sessions.Select(s => s.SessionId).Distinct().ToList();
        var existingSessions = new List<string?>();

        const int batchSize = 20;
        for (int i = 0; i < sessionIds.Count; i += batchSize)
        {
            var batch = sessionIds.Skip(i).Take(batchSize);
            var batchIds = string.Join(",", batch.Select(id => $"'{id}'"));
            var query = $"BeyondTrustAccessSession_CL | where SessionId in ({batchIds}) | project SessionId";
            var existingSessionResult = await queryService.QueryWorkspace(query);
            if (existingSessionResult?.Table.Rows is not null)
            {
                existingSessions.AddRange(existingSessionResult.Table.Rows.Select(r => r[0].ToString()));
            }
        }

        return existingSessions.Distinct().ToList();
    }

    private async Task<DateTime?> GetLastUpdatedTime(int offsetSeconds = 1)
    {
        var result = await queryService.QueryWorkspace("BeyondTrustAccessSession_CL | summarize arg_max(TimeGenerated,*) | project TimeGenerated");
        DateTime? lastEventTime = null;
        if (result?.Table.Rows.Count == 1 && result?.Table.Rows[0][0] is not null)
        {
            lastEventTime = ((DateTimeOffset)result.Table.Rows[0][0]).UtcDateTime;
            lastEventTime = lastEventTime.Value.AddSeconds(offsetSeconds);
        }
        if (lastEventTime is null)
            logger.LogWarning("No last updated timestamp found");
        else
            logger.LogInformation("Last event time: {LastEventTime}", lastEventTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

        return lastEventTime;
    }
}
