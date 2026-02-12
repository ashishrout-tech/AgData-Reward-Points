namespace Project.Application.DTOs.Event
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public Guid? PhotoId { get; set; }
		public string Title { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EventDetailDto
    {
        public Guid Id { get; set; }
        public Guid? PhotoId { get; set; }
		public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationInMinutes { get; set; }
        public bool IsCancelled { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerName { get; set; } = null!;
        public List<string> Tags { get; set; } = new();
        public int ParticipantCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EventParticipantDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
		public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public int Role { get; set; }
        public int? Rank { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class EventParticipantDetailDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public int Role { get; set; }
        public int? Rank { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class EventStatisticsDto
    {
        public Guid EventId { get; set; }
        public Guid? PhotoId { get; set; }
		public string EventTitle { get; set; } = null!;
		public int TotalParticipants { get; set; }
        public int OrganizersCount { get; set; }
        public int SpeakersCount { get; set; }
        public int AttendeesCount { get; set; }
        public double DurationInMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsCancelled { get; set; }
    }
}
