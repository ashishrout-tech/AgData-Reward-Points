using Project.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities.Event
{
    public class EventMetadata
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid OrganizerId { get; private set; }
        public Event Event { get; private set; } = null!;
        public User Organizer { get; private set; } = null!;
        public List<string> Tags { get; private set; } = new();
        public EventMetadata() { }

        public EventMetadata(Guid organizerId)
        {
            OrganizerId = organizerId;
        }

        public void AddTag(string tag)
        {
            if (!Tags.Contains(tag))
                Tags.Add(tag);
        }
    }
}
