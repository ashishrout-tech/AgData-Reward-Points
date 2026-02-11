using System;

namespace Project.Application.DTOs.Redemption
{
	public class RedemptionDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public Guid ProductId { get; set; }
		public string ProductName { get; set; }
		public int PointsRequired { get; set; }
		public int Status { get; set; }
		public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public DateTime? RejectedOn { get; set; }
	}
	public class RedemptionPendingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int PointsRequired { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

	public class RedemptionApprovedDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public Guid ProductId { get; set; }
		public string ProductName { get; set; }
		public int PointsRequired { get; set; }
		public int Status { get; set; }
		public DateTime CreatedAt { get; set; }
        public Guid ApprovedBy { get; set; }
		public string ApprovedByName { get; set; }
		public DateTime? ApprovedOn { get; set; }
	}

	public class RedemptionRejectedDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public Guid ProductId { get; set; }
		public string ProductName { get; set; }
		public int PointsRequired { get; set; }
		public int Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public Guid RejectedBy { get; set; }
        public string RejectedByName { get; set; }
		public DateTime? RejectedOn { get; set; }
	}
	public class RedemptionDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserBasicDto User { get; set; }
        public Guid ProductId { get; set; }
        public ProductBasicDto Product { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
		public DateTime? ApprovedOn { get; set; }
        public Guid? RejectedBy { get; set; }
        public string? RejectedByName { get; set; }
		public DateTime? RejectedOn { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class RedemptionApproveResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int PointsRequired { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ApprovedBy { get; set; }
        public DateTime ApprovedOn { get; set; }
    }

    public class RedemptionRejectResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid RejectedBy { get; set; }
        public DateTime RejectedOn { get; set; }
        public string RejectionReason { get; set; }
        public RefundTransactionDto? RefundedTransaction { get; set; }
    }

    public class RefundTransactionDto
    {
        public Guid Id { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; }
        public int Source { get; set; }
    }

    public class RedemptionStatisticsDto
    {
        public int TotalRedemptions { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public int TotalPointsRefunded { get; set; }
        public double ConversionRate { get; set; }
        public TopProductDto TopProduct { get; set; }
    }

    public class TopProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int RedemptionCount { get; set; }
    }

    public class RejectedRedemptionReportDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string Reason { get; set; }
        public Guid RejectedBy { get; set; }
        public DateTime RejectedOn { get; set; }
        public bool Refunded { get; set; }
        public int RefundAmount { get; set; }
    }

    public class UserBasicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class ProductBasicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PointsRequired { get; set; }
        public string Brand { get; set; }
    }
}
