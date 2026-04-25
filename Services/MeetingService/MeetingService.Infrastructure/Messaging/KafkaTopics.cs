namespace MeetingService.Infrastructure.Messaging
{
    public static class KafkaTopics
    {
        public const string MeetingCreated = "meeting-created-events";
        public const string MeetingStarted = "meeting-started-events";
        public const string MeetingEnded = "meeting-ended-events";
        public const string MeetingDeleted = "meeting-deleted-events";
        public const string ParticipantJoined = "participant-joined-events";
        public const string ParticipantLeft = "participant-left-events";
        public const string MeetingInvited = "meeting-invited-events";
        public const string InviteResponded = "invite-responded-events";
    }
}
