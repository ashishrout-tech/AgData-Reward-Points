using AGDATA.Rewards.Core.UserAggregate;
using Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities.Event
{
    public class EventParticipant
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid UserId { get; private set; }
        public ParticipantRole Role { get; private set; } = ParticipantRole.Attendee;
        public int? Rank { get; private set; }
        public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
        public Event Event { get; private set; } = null!;
        public User User { get; private set; } = null!;
        private EventParticipant() { }

        public EventParticipant(Guid eventId, Guid userId, ParticipantRole role = ParticipantRole.Attendee)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("EventId cannot be empty.", nameof(eventId));
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (!Enum.IsDefined(typeof(ParticipantRole), role))
                throw new ArgumentException("Invalid participant role.", nameof(role));

            Id = Guid.NewGuid();
            EventId = eventId;
            UserId = userId;
            Role = role;
            JoinedAt = DateTime.UtcNow;
        }

        public void UpdateRole(ParticipantRole role)
        {
            if (!Enum.IsDefined(typeof(ParticipantRole), role))
                throw new ArgumentException("Invalid participant role.", nameof(role));
            Role = role;
        }

        public void AssignRank(int rank)
        {
            if (rank <= 0)
                throw new ArgumentException("Rank must be positive.", nameof(rank));
            Rank = rank;
        }
    }
}
