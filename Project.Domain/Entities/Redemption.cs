using Project.Domain.Enums;
using Project.Domain.Entities.Users;
using ProductEntity = Project.Domain.Entities.Product.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Redemption
    {
        // Primary keys
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid ProductId { get; private set; }

        // Status tracking
        public RedemptionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Approval tracking
        public Guid? ApprovedBy { get; private set; }
        public DateTime? ApprovedOn { get; private set; }

        // Rejection tracking
        public Guid? RejectedBy { get; private set; }
        public DateTime? RejectedOn { get; private set; }
        public string? RejectionReason { get; private set; }

        // Transaction tracking
        public Guid? DeductionTransactionId { get; private set; }
        public Guid? RefundTransactionId { get; private set; }

        // Navigation properties
        public User User { get; private set; } = null!;
        public ProductEntity Product { get; private set; } = null!;
        public User? ApprovalAdmin { get; private set; }
        public User? RejectionAdmin { get; private set; }
        public Transaction? DeductionTransaction { get; private set; }
        public Transaction? RefundTransaction { get; private set; }

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
            ApprovedBy = approvedBy;
            ApprovedOn = DateTime.UtcNow;
        }

        public void Reject(Guid rejectedBy, string reason)
        {
            if (Status != RedemptionStatus.Pending)
                throw new InvalidOperationException("Redemption can only be rejected from Pending status.");
            if (rejectedBy == Guid.Empty)
                throw new ArgumentException("RejectedBy cannot be empty.", nameof(rejectedBy));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason cannot be null or whitespace.", nameof(reason));

            Status = RedemptionStatus.Rejected;
            RejectedBy = rejectedBy;
            RejectedOn = DateTime.UtcNow;
            RejectionReason = reason;
        }

        public void LinkDeductionTransaction(Guid transactionId)
        {
            if (Status != RedemptionStatus.Approved)
                throw new InvalidOperationException("Can only link deduction transaction to approved redemption.");
            if (transactionId == Guid.Empty)
                throw new ArgumentException("TransactionId cannot be empty.", nameof(transactionId));
            
            DeductionTransactionId = transactionId;
        }

        public void LinkRefundTransaction(Guid transactionId)
        {
            if (Status != RedemptionStatus.Rejected)
                throw new InvalidOperationException("Can only link refund transaction to rejected redemption.");
            if (transactionId == Guid.Empty)
                throw new ArgumentException("TransactionId cannot be empty.", nameof(transactionId));
            
            RefundTransactionId = transactionId;
        }
    }
}
