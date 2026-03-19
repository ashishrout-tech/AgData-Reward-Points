namespace AGDATA.Rewards.Core.EventAggregate;

public sealed class EventMetadata
{
  public Guid EventId { get; }
  public Guid OrganizerId { get; }
  private readonly List<string> _tags = [];
  public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();
  private EventMetadata() { }

  internal EventMetadata(Guid eventId, Guid organizerId)
  {
    EventId = eventId;
    OrganizerId = organizerId;
  }

  internal void AddTag(string tag)
  {
    if(tag.Length > 20)
      throw new ArgumentException("Tag cannot exceed 20 characters.", nameof(tag));

    if (tag.Split(null as char[], StringSplitOptions.RemoveEmptyEntries).Length > 1)
      throw new ArgumentException("Tag can only be one word.", nameof(tag));

    if (!_tags.Contains(tag))
      _tags.Add(tag);
  }

  internal void RemoveTag(string tag)
  {
    _tags.Remove(tag);
  }
}
