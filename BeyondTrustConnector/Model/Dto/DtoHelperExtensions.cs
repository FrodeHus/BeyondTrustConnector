namespace BeyondTrustConnector.Model.Dto
{
    internal static class DtoHelperExtensions
    {
        internal static BeyondTrustAccessSessionDto ToDto(this session_listSession session)
        {
            var jumpGroup = session.jump_group.Value;
            var sessionId = session.lsid;
            var startTime = DateTimeOffset.Parse(session.start_time.Value);
            DateTimeOffset? endTime = null;
            var endTimeValue = session.end_time?.Value;
            if (!string.IsNullOrEmpty(endTimeValue))
            {
                endTime = DateTimeOffset.Parse(endTimeValue);
            }

            var jumpoint = session.jumpoint.Value;
            var jumpItem = session.primary_customer.Value;
            var sessionType = session.session_type;
            return new BeyondTrustAccessSessionDto
            {
                SessionId = sessionId,
                SessionType = sessionType,
                StartTime = startTime.UtcDateTime,
                EndTime = endTime?.UtcDateTime,
                Jumpoint = jumpoint,
                JumpItemId = int.Parse(session.primary_customer.gsnumber),
                JumpItemAddress = jumpItem,
                JumpGroup = jumpGroup,
                JumpGroupId = int.Parse(session.jump_group.id),
                FileDeleteCount = int.Parse(session.file_delete_count),
                FileMoveCount = int.Parse(session.file_move_count),
                FileTransferCount = int.Parse(session.file_transfer_count),
                ChatDownloadUrl = session.session_chat_download_url,
                ChatViewUrl = session.session_chat_view_url,
                UserDetails = session.rep_list.Select(userDetails => new Dictionary<string, object>
                    {
                        { "Username", userDetails.username },
                        { "PublicIP", userDetails.public_ip },
                        { "PrivateIP", userDetails.private_ip },
                        { "Hostname", userDetails.hostname },
                        { "OS", userDetails.os },
                        { "SessionOwner", userDetails.session_owner == "1" },
                        { "SecondsInvolved", userDetails.seconds_involved }
                    }
                ).ToList(),
                Events = session.session_details.Select(sessionDetails => new Dictionary<string, object>
                    {
                        { "Event", sessionDetails.event_type },
                        { "When", sessionDetails.timestamp },
                        { "Who", sessionDetails.performed_by?.Value ?? "Unknown" },
                        { "Destination", sessionDetails.destination?.Value ?? "Unknown" },
                        { "Details", sessionDetails.data?.Select(detail => new Dictionary<string, object>
                            {
                                { detail.value.name, detail.value.value }
                            }
                        ).ToList() ?? []}
                    }
                ).ToList()
            };
        }
    }
}
