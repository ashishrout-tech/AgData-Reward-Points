using System;

namespace Project.Application.DTOs.Transaction
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int Type { get; set; }
        public int Source { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid? EventId { get; set; }
        public Guid? EventParticipantId { get; set; }
        public int? Rank { get; set; }
        public bool IsReversed { get; set; }
    }

    public class TransactionDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int Type { get; set; }
        public int Source { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid? EventId { get; set; }
        public Guid? EventParticipantId { get; set; }
        public int? Rank { get; set; }
        public Guid? AdminApprovedBy { get; set; }
        public bool IsReversed { get; set; }
        public string? ReversalReason { get; set; }
        public DateTime? ReversedOn { get; set; }
        public Guid? ReversedBy { get; set; }
        public TransactionUserDto? User { get; set; }
        public TransactionEventDto? Event { get; set; }
    }

    public class EventEarningsReportDto
    {
        public Guid EventId { get; set; }
        public string EventTitle { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalRecords { get; set; }
        public List<EventEarningItemDto> Earnings { get; set; }
    }

    public class EventEarningItemDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int Rank { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class ReversedTransactionReportDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int OriginalPoints { get; set; }
        public string OriginalReason { get; set; }
        public string ReversalReason { get; set; }
        public Guid ReversedBy { get; set; }
        public DateTime ReversedOn { get; set; }
    }

    public class TransactionUserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class TransactionEventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }
}
