using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities.Event
{
    public class Event
    {
        public Guid Id {  get; private set; }
        public Guid EventMetadataId { get; private set; }
        public Guid EventScheduleId { get; private set; }
        public string Title { get; private set; } = null!;
        public bool IsCancelled { get; private set; }
        public ICollection<EventParticipant> Participants { get; private set; } = new List<EventParticipant>();
        public EventMetadata EventMetadata { get; private set; } = null!;
        public EventSchedule EventSchedule { get; private set; } = null!;
        private Event() { }

        public Event(string title, string description, EventMetadata metadata, EventSchedule schedule)
        {
            Id = Guid.NewGuid();
            Title = title;
            EventMetadata = metadata;
            EventSchedule = schedule;
        }

        public void SetMetadata(EventMetadata metadata)
        {
            EventMetadata = metadata;
        }

        public void SetSchedule(EventSchedule schedule)
        {
            EventSchedule = schedule;
        }

        public void AddParticipant(EventParticipant participant)
        {
            if (IsCancelled)
                throw new InvalidOperationException("Cannot add participants to a cancelled event.");

            if (Participants.Any(p => p.UserId == participant.UserId))
                throw new InvalidOperationException("Participant already registered for this event.");

            Participants.Add(participant);
        }

        public void RemoveParticipant(Guid userId)
        {
            var participant = Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                throw new InvalidOperationException("Participant not found in this event.");

            Participants.Remove(participant);
        }

        public void CancelEvent()
        {
            IsCancelled = true;
        }
    }
}
