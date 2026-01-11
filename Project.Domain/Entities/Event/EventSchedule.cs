using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities.Event
{
    public class EventSchedule
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Event Event { get; private set; } = null!;
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public EventSchedule() { }

        public EventSchedule(DateTime startTime, DateTime endTime)
        {
            if (startTime >= endTime)
                throw new ArgumentException("Start time must be before end time.");

            StartTime = startTime;
            EndTime = endTime;
        }

        public double DurationInMinutes()
        {
            return (EndTime - StartTime).TotalMinutes;
        }

        public void Reschedule(DateTime newStart, DateTime newEnd)
        {
            if (newStart >= newEnd)
                throw new ArgumentException("New start time must be before end time.");
            StartTime = newStart;
            EndTime = newEnd;
        }
    }
}
