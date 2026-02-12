using System;

namespace Project.Application.DTOs.Transaction
{
    public class RecordEventEarningRequest
    {
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public Guid EventParticipantId { get; set; }
        public int Points { get; set; }
        public int Rank { get; set; }
        public string Reason { get; set; }
    }

    public class AwardPointsRequest
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; }
    }

    public class ReverseTransactionRequest
    {
        public string Reason { get; set; }
    }
}
