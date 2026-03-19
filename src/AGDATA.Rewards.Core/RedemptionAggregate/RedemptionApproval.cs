namespace AGDATA.Rewards.Core.RedemptionAggregate;

public sealed class RedemptionApproval
{
  public Guid RedemptionId { get; }
  public Guid ApprovedBy { get; }
  public DateTime ApprovedOn { get; }

  private RedemptionApproval() { }
  internal RedemptionApproval(Guid redemptionId, Guid approvedBy)
  {
    RedemptionId = redemptionId;
    ApprovedBy = approvedBy;
    ApprovedOn = DateTime.UtcNow;
  }
}
