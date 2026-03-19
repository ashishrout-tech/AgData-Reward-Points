namespace AGDATA.Rewards.Core.EventAggregate;

public sealed class EventParticipant
{
  public Guid EventId { get; }
  public Guid UserId { get; }
  public EventParticipantRole Role { get; private set; }
  public int? Rank { get; private set; }
  public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
  private EventParticipant() { }

  internal EventParticipant(Guid eventId, Guid userId, EventParticipantRole role = EventParticipantRole.Attendee)
  {
    EventId = eventId;
    UserId = userId;
    Role = role;
    JoinedAt = DateTime.UtcNow;
  }

  internal void UpdateRole(EventParticipantRole role) => Role = role;

  internal void AssignRank(int rank) => Rank = rank;
}
