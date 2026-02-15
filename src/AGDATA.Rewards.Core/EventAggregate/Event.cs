using System.Security.AccessControl;
using AGDATA.Rewards.Core.Common;
using AGDATA.Rewards.Core.EventAggregate.ValueObjects;
using AGDATA.Rewards.Core.UserAggregate;
using Project.Domain.Entities;
using Project.Domain.Enums;

namespace AGDATA.Rewards.Core.EventAggregate;

public sealed class Event : IAggregateRoot
{
  public Guid Id { get; }
  public EventTitle Title { get; private set; }
  public string Description { get; private set; }
  public bool IsCancelled { get; private set; }
  public Guid? PhotoId { get; private set; }
  private readonly List<EventParticipant> _participants = [];
  public IReadOnlyCollection<EventParticipant> Participants => _participants.AsReadOnly();
  public EventMetadata EventMetadata { get; private set; }
  public EventSchedule EventSchedule { get; private set; }
  public Photo? Photo { get; private set; }
  private Event() { }

  public Event(EventTitle title, string description)
  {
    if (string.IsNullOrWhiteSpace(description))
      throw new ArgumentException("Event description cannot be null or empty.", nameof(description));

    Id = Guid.NewGuid();
    Title = title;
    Description = description;
  }

  public void UpdateDetails(EventTitle? title, string? description)
  {
    ThrowIfCancelled();
    Title = title ?? Title;
    Description = description ?? Description;
  }
  public void SetPhoto(Guid PhotoId)
  {
    ThrowIfCancelled();
    if (PhotoId == Guid.Empty)
      throw new ArgumentException("PhotoId cannot be empty.", nameof(PhotoId));
    this.PhotoId = PhotoId;
  }
  public void SetMetadata(EventMetadata metadata)
  {
    EventMetadata = metadata;
  }

  public void SetSchedule(EventSchedule schedule)
  {
    EventSchedule = schedule;
  }

  public void AddParticipant(Guid userId)
  {
    ThrowIfCancelled();

    if(_participants.Any(p => p.UserId == userId))
      throw new InvalidOperationException("Participant already registered for this event.");

    EventParticipant participant = new EventParticipant(Id, userId);
    _participants.Add(participant);
  }

  public void RemoveParticipant(Guid userId)
  {
    ThrowIfCancelled();
    var participant = _participants.FirstOrDefault(p => p.UserId == userId);

    if (participant == null)
      throw new InvalidOperationException("Participant not found in this event.");

    _participants.Remove(participant);
  }

  public void CancelEvent() => IsCancelled = true;

  private void ThrowIfCancelled()
  {
    if (IsCancelled)
      throw new InvalidOperationException("This event has been cancelled.");
  }
}

public sealed class EventParticipant
{
  public Guid EventId { get; private set; }
  public Guid UserId { get; private set; }
  public ParticipantRole Role { get; private set; }
  public int? Rank { get; private set; }
  public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
  private EventParticipant() { }

  internal EventParticipant(Guid eventId, Guid userId, ParticipantRole role = ParticipantRole.Attendee)
  {
    EventId = eventId;
    UserId = userId;
    Role = role;
    JoinedAt = DateTime.UtcNow;
  }

  public void UpdateRole(ParticipantRole role) => Role = role;

  public void AssignRank(int rank)
  {
    if (rank <= 0)
      throw new ArgumentException("Rank must be positive.", nameof(rank));
    if(rank > 1000)
      throw new ArgumentException("Rank cannot exceed 1000.", nameof(rank));
    Rank = rank;
  }
}

public class EventMetadata
{
  public Guid EventId { get; set; }
  public Guid OrganizerId { get; private set; }
  public List<string> Tags { get; private set; } = new();
  public EventMetadata() { }

  internal EventMetadata(Guid eventId, Guid organizerId)
  {
    EventId = eventId;
    OrganizerId = organizerId;
  }

  internal void AddTag(string tag)
  {
    if (!Tags.Contains(tag))
      Tags.Add(tag);
  }

  internal void RemoveTag(string tag)
  {
    Tags.Remove(tag);
  }
}

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
