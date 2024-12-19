using System.Globalization;
using System.Xml.Linq;
using BeyondTrustConnector.Model;
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

        var ns = "http://www.beyondtrust.com/sra/namespaces/API/reporting";
        var report = await beyondTrustService.GetAccessSessionReport(lastEventTime.Value);
        var sessions = report.Root!.Descendants(XName.Get("session", ns));
        var existingSessions = await CheckIfSessionsAlreadyExists(queryService, sessions);
        var accessSessions = new List<BeyondTrustAccessSession>();

        foreach (var session in sessions)
        {
            if (session is null) continue;

            var sessionId = session.Attribute("lsid")!.Value;
            if (existingSessions.Contains(sessionId))
            {
                logger.LogInformation("Session {SessionId} already exists in the workspace", sessionId);
                continue;
            }

            var sessionData = CreateSessionData(session, ns);
            if (sessionData != null)
            {
                accessSessions.Add(sessionData);
            }
        }

        if (accessSessions.Count != 0)
        {
            await ingestionService.IngestAccessSessions(accessSessions);
        }
    }

    private BeyondTrustAccessSession? CreateSessionData(XElement session, string ns)
    {
        try
        {
            var sessionId = session.Attribute("lsid")!.Value;
            var startTime = DateTimeOffset.Parse(session.Element(XName.Get("start_time", ns))!.Value);
            DateTimeOffset? endTime = null;
            var endTimeValue = session.Element(XName.Get("end_time", ns));
            if (!string.IsNullOrEmpty(endTimeValue?.Value))
            {
                endTime = DateTimeOffset.Parse(endTimeValue!.Value);
            }

            var jumpPoint = session.Element(XName.Get("jumpoint", ns))!.Value;
            var jumpItem = session.Element(XName.Get("primary_customer", ns));
            var jumpItemValue = jumpItem!.Value;
            var jumpItemId = jumpItem!.Attribute("gsnumber")!.Value;
            var jumpGroup = session.Element(XName.Get("jump_group", ns))!.Value;
            var sessionType = session.Element(XName.Get("session_type", ns))!.Value;

            var sessionData = new BeyondTrustAccessSession
            {
                StartTime = startTime.UtcDateTime,
                EndTime = endTime?.UtcDateTime,
                SessionId = sessionId,
                Jumpoint = jumpPoint,
                JumpItemAddress = jumpItemValue,
                JumpItemId = int.Parse(jumpItemId),
                JumpGroup = jumpGroup,
                SessionType = sessionType
            };

            if (int.TryParse(session.Element(XName.Get("file_transfer_count", ns))?.Value, out var fileTransferCount))
            {
                sessionData.FileTransferCount = fileTransferCount;
            }

            if (int.TryParse(session.Element(XName.Get("file_move_count", ns))?.Value, out var fileMoveCount))
            {
                sessionData.FileMoveCount = fileMoveCount;
            }

            if (int.TryParse(session.Element(XName.Get("file_move_count", ns))?.Value, out var fileDeleteCount))
            {
                sessionData.FileDeleteCount = fileDeleteCount;
            }

            sessionData.UserDetails = GetUserDetails(session, ns);
            return sessionData;
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating session data: {ErrorMessage}", ex.Message);
            return null;
        }
    }

    private static async Task<List<string?>> CheckIfSessionsAlreadyExists(QueryService queryService, IEnumerable<XElement> sessions)
    {
        var sessionIds = sessions.Select(s => s.Attribute("lsid")!.Value).Distinct().ToList();
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

    private static List<Dictionary<string, object>> GetUserDetails(XElement session, string ns)
    {
        var users = new List<Dictionary<string, object>>();
        var userList = session.Element(XName.Get("rep_list", ns));
        if (userList is null) return users;

        foreach (var userElement in userList.Elements(XName.Get("representative", ns)))
        {
            var userDetails = new Dictionary<string, object>();
            if (userElement is not null)
            {
                userDetails.Add("Username", userElement.Element(XName.Get("username", ns))?.Value ?? "Unknown");
                userDetails.Add("PublicIP", userElement.Element(XName.Get("public_ip", ns))?.Value ?? "Unknown");
                userDetails.Add("PrivateIP", userElement.Element(XName.Get("private_ip", ns))?.Value ?? "Unknown");
                userDetails.Add("Hostname", userElement.Element(XName.Get("hostname", ns))?.Value ?? "Unknown");
                userDetails.Add("OS", userElement.Element(XName.Get("os", ns))?.Value ?? "Unknown");
                var sessionOwnerData = userElement.Element(XName.Get("session_owner", ns))?.Value;
                if (!string.IsNullOrEmpty(sessionOwnerData))
                {
                    userDetails.Add("SessionOwner", sessionOwnerData == "1");
                }
            }
            users.Add(userDetails);
        }
        return users;
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
