using AGDATA.Rewards.Core.Common;
using AGDATA.Rewards.Core.EventAggregate;
using AGDATA.Rewards.Core.UserAggregate;
using Project.Domain.Enums;

namespace AGDATA.Rewards.Core.TransactionAggregate;

public sealed class Transaction : IAggregateRoot
{
  // Primary keys
  public Guid Id { get; }
  public Guid UserId { get; }

  // Transaction metadata
  public TransactionType Type { get; }
  public int Points { get; }
  public DateTime TimeStamp { get; }

  // Earn - Event-related (nullable - only for Event source)
  public Guid? EventId { get; private set; }

  // Redemption-related (nullable - only for Redeem type or RedemptionRefund source)
  public Guid? RedemptionId { get; private set; }

  // Reversal tracking
  public bool IsReversed { get; private set; }
  public string? ReversalReason { get; private set; }
  public DateTime? ReversedOn { get; private set; }
  public Guid? ReversedBy { get; private set; }

  private Transaction() { }

  public Transaction(Guid userId, Guid eventId, Guid eventParticipantId,
      int points, int rank, string reason)
  {
    ValidateUserId(userId);
    ValidatePoints(points);
    ValidateReason(reason);

    if (eventId == Guid.Empty)
      throw new ArgumentException("EventId cannot be empty.", nameof(eventId));
    if (eventParticipantId == Guid.Empty)
      throw new ArgumentException("EventParticipantId cannot be empty.", nameof(eventParticipantId));
    if (rank <= 0)
      throw new ArgumentException("Rank must be positive.", nameof(rank));

    Id = Guid.NewGuid();
    UserId = userId;
    Type = TransactionType.Earn;
    Points = points;
    TimeStamp = DateTime.UtcNow;
    EventId = eventId;
    EventParticipantId = eventParticipantId;
    Rank = rank;
    IsReversed = false;
  }

  public Transaction(Guid userId, int points, string reason, Guid adminApprovedBy, bool isAdminAward)
  {
    ValidateUserId(userId);
    ValidatePoints(points);
    ValidateReason(reason);

    if (adminApprovedBy == Guid.Empty)
      throw new ArgumentException("AdminApprovedBy cannot be empty.", nameof(adminApprovedBy));

    Id = Guid.NewGuid();
    UserId = userId;
    Type = TransactionType.Earn;
    Source = TransactionSource.AdminAward;
    Points = points;
    Reason = reason;
    TimeStamp = DateTime.UtcNow;
    AdminApprovedBy = adminApprovedBy;
    IsReversed = false;
  }

  public static Transaction CreateRedemptionDeduction(Guid userId, int points, string reason, Guid redemptionId)
  {
    var transaction = new Transaction();
    transaction.ValidateUserId(userId);
    transaction.ValidatePoints(points);
    transaction.ValidateReason(reason);

    if (redemptionId == Guid.Empty)
      throw new ArgumentException("RedemptionId cannot be empty.", nameof(redemptionId));

    transaction.Id = Guid.NewGuid();
    transaction.UserId = userId;
    transaction.Type = TransactionType.Redeem;
    transaction.Points = points;
    transaction.Reason = reason;
    transaction.TimeStamp = DateTime.UtcNow;
    transaction.RedemptionId = redemptionId;
    transaction.IsReversed = false;

    return transaction;
  }

  public static Transaction CreateRedemptionRefund(Guid userId, int points, string reason, Guid redemptionId)
  {
    var transaction = new Transaction();
    transaction.ValidateUserId(userId);
    transaction.ValidatePoints(points);
    transaction.ValidateReason(reason);

    if (redemptionId == Guid.Empty)
      throw new ArgumentException("RedemptionId cannot be empty.", nameof(redemptionId));

    transaction.Id = Guid.NewGuid();
    transaction.UserId = userId;
    transaction.Type = TransactionType.Earn;
    transaction.Source = TransactionSource.RedemptionRefund;
    transaction.Points = points;
    transaction.Reason = reason;
    transaction.TimeStamp = DateTime.UtcNow;
    transaction.RedemptionId = redemptionId;
    transaction.IsReversed = false;

    return transaction;
  }

  public void ReverseTransaction(Guid reversedBy, string reason)
  {
    if (IsReversed)
      throw new InvalidOperationException("Transaction already reversed.");

    if (reversedBy == Guid.Empty)
      throw new ArgumentException("ReversedBy cannot be empty.", nameof(reversedBy));

    ValidateReason(reason);

    IsReversed = true;
    ReversedOn = DateTime.UtcNow;
    ReversedBy = reversedBy;
    ReversalReason = reason;
  }

  private void ValidateUserId(Guid userId)
  {
    if (userId == Guid.Empty)
      throw new ArgumentException("UserId cannot be empty.", nameof(userId));
  }

  private void ValidatePoints(int points)
  {
    if (points <= 0)
      throw new ArgumentOutOfRangeException(nameof(points), "Points must be greater than zero.");
  }

  private void ValidateReason(string reason)
  {
    if (string.IsNullOrWhiteSpace(reason))
      throw new ArgumentException("Reason cannot be null or whitespace.", nameof(reason));
  }
}
