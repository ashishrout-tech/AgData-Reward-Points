using AGDATA.Rewards.Core.Common;

namespace AGDATA.Rewards.Core.RedemptionAggregate;

public sealed class Redemption : IAggregateRoot
{
  public Guid Id { get; }
  public Guid UserId { get; }
  public Guid ProductId { get; }
  public RedemptionStatus Status { get; private set; }
  public DateTime CreatedAt { get; }

  public RedemptionApproval? Approval { get; private set; }
  public RedemptionRejection? Rejection { get; private set; }

  private Redemption() { }

  public Redemption(Guid userId, Guid productId)
  {
    if (userId == Guid.Empty)
      throw new ArgumentException("UserId cannot be empty.", nameof(userId));
    if (productId == Guid.Empty)
      throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

    Id = Guid.NewGuid();
    UserId = userId;
    ProductId = productId;
    Status = RedemptionStatus.Pending;
    CreatedAt = DateTime.UtcNow;
  }

  public void Approve(Guid approvedBy)
  {
    if (Status != RedemptionStatus.Pending)
      throw new InvalidOperationException("Redemption can only be approved from Pending status.");
    if (approvedBy == Guid.Empty)
      throw new ArgumentException("ApprovedBy cannot be empty.", nameof(approvedBy));

    Status = RedemptionStatus.Approved;
    Approval = new RedemptionApproval(Id, approvedBy);
  }

  public void Reject(Guid rejectedBy, string? reason)
  {
    if (Status != RedemptionStatus.Pending)
      throw new InvalidOperationException("Redemption can only be rejected from Pending status.");
    if (rejectedBy == Guid.Empty)
      throw new ArgumentException("RejectedBy cannot be empty.", nameof(rejectedBy));

    Status = RedemptionStatus.Rejected;
    Rejection = new RedemptionRejection(Id, rejectedBy, reason);
  }
}
