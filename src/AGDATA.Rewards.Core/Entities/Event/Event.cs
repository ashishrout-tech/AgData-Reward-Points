//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Project.Domain.Entities.Event
//{
//    public class Event
//    {
//        public Guid Id {  get; private set; }
//        public string Title { get; private set; } = null!;
//        public string Description { get; private set; } = null!;
//		public bool IsCancelled { get; private set; }
//        public Guid? PhotoId { get; private set; }
//        public ICollection<EventParticipant> Participants { get; private set; } = new List<EventParticipant>();
//        public EventMetadata EventMetadata { get; private set; } = null!;
//        public EventSchedule EventSchedule { get; private set; } = null!;
//        public Photo? Photo { get; private set; }
//		private Event() { }

//        public Event(string title, string description, EventMetadata metadata, EventSchedule schedule)
//        {
//            Id = Guid.NewGuid();
//            Title = title;
//            Description = description;
//			EventMetadata = metadata;
//            EventSchedule = schedule;
//        }

//        public void SetTitle(string title)
//        {
//            Title = title;
//		}
//        public void SetDescription(string description)
//        {
//            Description = description;
//		}
//        public void SetPhoto(Guid PhotoId)
//        {
//            if(PhotoId == Guid.Empty)
//                throw new ArgumentException("PhotoId cannot be empty.", nameof(PhotoId));
//            this.PhotoId = PhotoId;
//		}
//		public void SetMetadata(EventMetadata metadata)
//        {
//            EventMetadata = metadata;
//        }

//        public void SetSchedule(EventSchedule schedule)
//        {
//            EventSchedule = schedule;
//        }

//        public void AddParticipant(EventParticipant participant)
//        {
//            if (IsCancelled)
//                throw new InvalidOperationException("Cannot add participants to a cancelled event.");

//            if (Participants.Any(p => p.UserId == participant.UserId))
//                throw new InvalidOperationException("Participant already registered for this event.");

//            Participants.Add(participant);
//        }

//        public void RemoveParticipant(Guid userId)
//        {
//            var participant = Participants.FirstOrDefault(p => p.UserId == userId);
//            if (participant == null)
//                throw new InvalidOperationException("Participant not found in this event.");

//            Participants.Remove(participant);
//        }

//        public void CancelEvent()
//        {
//            IsCancelled = true;
//        }
//    }
//}
