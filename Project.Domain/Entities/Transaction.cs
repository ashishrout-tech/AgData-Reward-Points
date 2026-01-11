using Project.Domain.Enums;
using Project.Domain.Entities.Users;
using EventEntity = Project.Domain.Entities.Event.Event;
using Project.Domain.Entities.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Transaction
    {
        // Primary keys
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }

        // Transaction metadata
        public TransactionType Type { get; private set; }
        public TransactionSource Source { get; private set; }
        public int Points { get; private set; }
        public DateTime TimeStamp { get; private set; }

        // Event-related (nullable - only for Event source)
        public Guid? EventId { get; private set; }
        public Guid? EventParticipantId { get; private set; }
        public int? Rank { get; private set; }

        // Admin-related (nullable - only for AdminAward source)
        public Guid? AdminApprovedBy { get; private set; }
        public string Reason { get; private set; } = null!;

        // Reversal tracking
        public bool IsReversed { get; private set; }
        public string? ReversalReason { get; private set; }
        public DateTime? ReversedOn { get; private set; }
        public Guid? ReversedBy { get; private set; }

        // Navigation properties
        public User User { get; private set; } = null!;
        public EventEntity? Event { get; private set; }
        public EventParticipant? EventParticipant { get; private set; }
        public User? AdminUser { get; private set; }
        public User? ReversalAdmin { get; private set; }

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
            Source = TransactionSource.Event;
            Points = points;
            Reason = reason;
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

        public static Transaction CreateRedemptionRefund(Guid userId, int points, string reason, Guid approvalAdmin)
        {
            var transaction = new Transaction();
            transaction.ValidateUserId(userId);
            transaction.ValidatePoints(points);
            transaction.ValidateReason(reason);

            transaction.Id = Guid.NewGuid();
            transaction.UserId = userId;
            transaction.Type = TransactionType.Earn;
            transaction.Source = TransactionSource.RedemptionRefund;
            transaction.Points = points;
            transaction.Reason = reason;
            transaction.TimeStamp = DateTime.UtcNow;
            transaction.AdminApprovedBy = approvalAdmin;
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
}
