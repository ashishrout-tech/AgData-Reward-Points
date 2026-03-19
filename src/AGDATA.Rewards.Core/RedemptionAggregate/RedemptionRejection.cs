namespace AGDATA.Rewards.Core.RedemptionAggregate;

public sealed class RedemptionRejection
{
  public Guid RedemptionId { get; }
  public Guid RejectedBy { get; }
  public DateTime RejectedOn { get; }
  public string? Reason { get; }

  private RedemptionRejection() { }
  internal RedemptionRejection(Guid redemptionId, Guid rejectedBy, string? reason)
  {
    RedemptionId = redemptionId;
    RejectedBy = rejectedBy;
    RejectedOn = DateTime.UtcNow;
    Reason = reason;
  }
}
