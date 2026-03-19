namespace AGDATA.Rewards.Core.EventAggregate;

public sealed class EventSchedule
{
  public Guid EventId { get; }
  public DateTime StartTime { get; private set; }
  public DateTime EndTime { get; private set; }
  private EventSchedule() { }

  internal EventSchedule(Guid eventId, DateTime startTime, DateTime endTime)
  {
    EventId = eventId;
    StartTime = startTime;
    EndTime = endTime;
  }

  internal int DurationInMinutes => (int)(EndTime - StartTime).TotalMinutes;

  internal void Reschedule(DateTime newStart, DateTime newEnd)
  {
    StartTime = newStart;
    EndTime = newEnd;
  }
}
