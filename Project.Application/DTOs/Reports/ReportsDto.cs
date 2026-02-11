using System;

namespace Project.Application.DTOs.Reports
{
    public class PointsSummaryDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int CurrentPoints { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public PointSourceBreakdownDto EventEarnings { get; set; }
        public PointSourceBreakdownDto AdminAwards { get; set; }
        public PointSourceBreakdownDto Refunds { get; set; }
        public RedemptionSummaryDto Redemptions { get; set; }
    }

    public class PointSourceBreakdownDto
    {
        public int Total { get; set; }
        public int Count { get; set; }
        public double Average { get; set; }
    }

    public class RedemptionSummaryDto
    {
        public int? Total { get; set; }
		public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int? PointsUsed { get; set; }
	}

    public class ActivityTimelineDto
    {
        public DateTime Date { get; set; }
        public List<ActivityTransactionDto> Transactions { get; set; }
    }

    public class ActivityTransactionDto
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public int? Points { get; set; }
        public TimeSpan TimeStamp { get; set; }
    }

    public class AdminDashboardDto
    {
        public TransactionDashboardDto Transactions { get; set; }
        public RedemptionDashboardDto Redemptions { get; set; }
        public List<TopEarnerDto> TopEarners { get; set; }
        public List<TopRedemptionProductDto> TopProducts { get; set; }
        public List<RecentActivityDto> RecentActivity { get; set; }
    }

    public class TransactionDashboardDto
    {
        public int TotalCount { get; set; }
        public int TodayCount { get; set; }
        public int EventEarnings { get; set; }
        public int AdminAwards { get; set; }
        public int Refunds { get; set; }
    }

    public class RedemptionDashboardDto
    {
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public double ConversionRate { get; set; }
    }

    public class TopEarnerDto
    {
        public Guid UserId { get; set; }
        public Guid? PhotoId { get; set; }
		public string Name { get; set; }
        public int TotalPoints { get; set; }
    }

    public class TopRedemptionProductDto
    {
        public Guid ProductId { get; set; }
        public Guid? PhotoId { get; set; }
		public string Name { get; set; }
        public int RedemptionCount { get; set; }
    }

    public class RecentActivityDto
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
