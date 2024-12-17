using System;
using System.Globalization;
using System.Xml.Linq;
using BeyondTrustConnector.Model;
using BeyondTrustConnector.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BeyondTrustConnector
{
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

            var accessSessions = new List<BeyondTrustAccessSession>();
            foreach (var session in sessions)
            {
                if (session is null) continue;
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
                var sessionId = session.Attribute("lsid")!.Value;

                var sessionData = new BeyondTrustAccessSession
                {
                    StartTime = startTime.UtcDateTime,
                    EndTime = endTime?.UtcDateTime,
                    SessionId = sessionId,
                    Jumpoint = jumpPoint,
                    JumpItemAddress = jumpItemValue,
                    JumpItemId = int.Parse(jumpItemId),
                    JumpGroup = jumpGroup,
                    SessionType = sessionType,
                    UserDetails = GetUserDetails(session, ns)
                };

                accessSessions.Add(sessionData);
            }

            await ingestionService.IngestAccessSessions(accessSessions);
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
                    userDetails.Add("Username", userElement.Element(XName.Get("username", ns))!.Value);
                    userDetails.Add("PublicIP", userElement.Element(XName.Get("public_ip", ns))!.Value);
                    userDetails.Add("PrivateIP", userElement.Element(XName.Get("private_ip", ns))!.Value);
                    userDetails.Add("Hostname", userElement.Element(XName.Get("hostname", ns))!.Value);
                    userDetails.Add("OS", userElement.Element(XName.Get("os", ns))!.Value);
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

        private async Task<DateTime?> GetLastUpdatedTime()
        {
            var result = await queryService.QueryWorkspace("BeyondTrustAccessSession_CL | summarize arg_max(TimeGenerated,*) | project TimeGenerated");
            DateTime? lastEventTime = null;
            if (result?.Table.Rows.Count == 1 && result?.Table.Rows[0][0] is not null)
            {
                lastEventTime = ((DateTimeOffset)result.Table.Rows[0][0]).UtcDateTime;
            }
            logger.LogInformation("Last event time: {LastEventTime}", lastEventTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            return lastEventTime;
        }
    }
}
