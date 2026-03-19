using AGDATA.Rewards.Core.Common;
using AGDATA.Rewards.Core.EventAggregate.ValueObjects;
using Project.Domain.Entities;

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
  public EventSchedule EventSchedule { get; private set; }
  public EventMetadata EventMetadata { get; private set; }
  public Photo? Photo { get; private set; }
  private Event() { }

  public Event(EventTitle title, string description, Guid organizerId, DateTime startTime, DateTime endTime)
  {
    if (string.IsNullOrWhiteSpace(description))
      throw new ArgumentException("Event description cannot be null or empty.", nameof(description));
    ThrowIfInaccurateDateTime(startTime, endTime);

    Id = Guid.NewGuid();
    Title = title;
    Description = description;
    EventSchedule = new EventSchedule(Id, startTime, endTime);
    EventMetadata = new EventMetadata(Id, organizerId);
    EventParticipant eventOrganizer = new(Id, organizerId, EventParticipantRole.Organizer);
    _participants.Add(eventOrganizer);
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

  public void UpdateSchedule(DateTime newStart, DateTime newEnd)
  {
    ThrowIfCancelled();
    ThrowIfInaccurateDateTime(newStart, newEnd);
    EventSchedule.Reschedule(newStart, newEnd);
  }

  public void AddTag(params string[] tags)
  {
    ThrowIfCancelled();
    foreach (var tag in tags)
      EventMetadata.AddTag(tag);
  }

  public void RemoveTag(params string[] tags)
  {
    ThrowIfCancelled();
    foreach (var tag in tags)
      EventMetadata.RemoveTag(tag);
  }

  public void AddParticipant(Guid userId)
  {
    ThrowIfCancelled();

    if(_participants.Any(p => p.UserId == userId))
      throw new InvalidOperationException("Participant already registered for this event.");

    EventParticipant participant = new(Id, userId);
    _participants.Add(participant);
  }

  public void RemoveParticipant(Guid userId)
  {
    ThrowIfCancelled();
    var participant = GetParticipant(userId);

    _participants.Remove(participant);
  }

  public void CancelEvent() => IsCancelled = true;

  public void UpdateParticipantRole(Guid userId, EventParticipantRole newRole)
  {
    ThrowIfCancelled();
    var participant = GetParticipant(userId);

    participant.UpdateRole(newRole);
  }

  public void AssignParticipantRank(Guid userId, int rank)
  {
    if(rank <= 0)
      throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be a positive integer.");
    if(rank > _participants.Count)
      throw new ArgumentOutOfRangeException(nameof(rank), "Rank cannot exceed the number of participants.");

    ThrowIfCancelled();
    var participant = GetParticipant(userId);
    participant.AssignRank(rank);
  }

  private EventParticipant GetParticipant(Guid userId)
  {
    var participant = _participants.FirstOrDefault(p => p.UserId == userId);
    if (participant == null)
      throw new InvalidOperationException("Participant not found in this event.");
    return participant;
  }
  
  private void ThrowIfCancelled()
  {
    if (IsCancelled)
      throw new InvalidOperationException("This event has been cancelled.");
  }
  private static void ThrowIfInaccurateDateTime(DateTime start, DateTime end)
  {
    if (start >= end)
      throw new ArgumentException("Start time must be before end time.");
    if (start < DateTime.UtcNow)
      throw new ArgumentException("Start time cannot be in the past.");
    if (end < DateTime.UtcNow)
      throw new ArgumentException("End time cannot be in the past.");
    if ((end - start).TotalDays > 365)
      throw new ArgumentException("Event duration cannot exceed 1 year.");
    if ((end - start).TotalMinutes < 15)
      throw new ArgumentException("Event duration must be at least 15 minutes.");
  }
}
