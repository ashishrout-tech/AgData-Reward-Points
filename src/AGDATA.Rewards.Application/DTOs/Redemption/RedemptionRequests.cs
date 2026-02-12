using System;

namespace Project.Application.DTOs.Redemption
{
    public class CreateRedemptionRequest
    {
        public Guid ProductId { get; set; }
    }

    public class ApproveRedemptionRequest
    {
    }

    public class RejectRedemptionRequest
    {
        public string Reason { get; set; }
        public bool RefundPoints { get; set; } = true;
    }
}
