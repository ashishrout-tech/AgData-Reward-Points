using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Reports;
using Project.Application.Services;
using Project.Domain.Interfaces;
using System.Security.Claims;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IUserAccountAsyncRepository _userAccountRepo;
        private readonly ITransactionAsyncRepository _transactionRepo;
        private readonly IRedemptionAsyncRepository _redemptionRepo;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            ITransactionService transactionService,
            IUserAccountAsyncRepository userAccountRepo,
            ITransactionAsyncRepository transactionRepo,
            IRedemptionAsyncRepository redemptionRepo,
            ILogger<ReportsController> logger)
        {
            _transactionService = transactionService;
            _userAccountRepo = userAccountRepo;
            _transactionRepo = transactionRepo;
            _redemptionRepo = redemptionRepo;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value == "Admin";
        }

        /// <summary>
        /// Get user points summary
        /// </summary>
        [HttpGet("user/{userId}/points-summary")]
        public async Task<ActionResult<PointsSummaryDto>> GetPointsSummary(
            Guid userId,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId && !IsAdmin())
                    return Forbid();

                _logger.LogInformation("Getting points summary for user: {UserId}", userId);

                var userAccount = await _userAccountRepo.GetAccountByUserIdAsync(userId, cancellationToken);
                var totalEarned = await _transactionRepo.GetTotalPointsEarnedAsync(userId, cancellationToken);
                var totalRedeemed = await _transactionRepo.GetTotalPointsRedeemedAsync(userId, cancellationToken);
                var eventEarnings = await _transactionRepo.GetUserEventEarningsAsync(userId, null, 0, 1000, cancellationToken);
                var adminAwards = await _transactionRepo.GetUserAdminAwardsAsync(userId, null, 0, 1000, cancellationToken);
                var refunds = await _redemptionRepo.GetUserRedemptionsByStatusAsync(userId, 2, 0, 1000, cancellationToken); // Status 2 = Rejected

                var summary = new PointsSummaryDto
                {
                    UserId = userId,
                    UserName = userAccount?.User?.Name ?? "Unknown",
                    CurrentPoints = userAccount?.Points ?? 0,
                    TotalEarned = totalEarned,
                    TotalRedeemed = totalRedeemed,
                    EventEarnings = new PointSourceBreakdownDto
                    {
                        Total = eventEarnings.Sum(t => t.Points),
                        Count = eventEarnings.Count,
                        Average = eventEarnings.Count > 0 ? eventEarnings.Average(t => t.Points) : 0
                    },
                    AdminAwards = new PointSourceBreakdownDto
                    {
                        Total = adminAwards.Sum(t => t.Points),
                        Count = adminAwards.Count,
                        Average = adminAwards.Count > 0 ? adminAwards.Average(t => t.Points) : 0
                    },
                    Refunds = new PointSourceBreakdownDto
                    {
                        Total = 0,
                        Count = refunds.Count,
                        Average = 0
                    },
                    Redemptions = new RedemptionSummaryDto
                    {
                        Pending = await _redemptionRepo.GetRedemptionCountByStatusAsync(0, cancellationToken),
                        Approved = await _redemptionRepo.GetRedemptionCountByStatusAsync(1, cancellationToken),
                        Rejected = await _redemptionRepo.GetRedemptionCountByStatusAsync(2, cancellationToken)
                    }
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving points summary: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving points summary" });
            }
        }

        /// <summary>
        /// Get user activity timeline
        /// </summary>
        [HttpGet("user/{userId}/activity-timeline")]
        public async Task<ActionResult<List<ActivityTimelineDto>>> GetActivityTimeline(
            Guid userId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId && !IsAdmin())
                    return Forbid();

                _logger.LogInformation("Getting activity timeline for user: {UserId}", userId);

                fromDate ??= DateTime.UtcNow.AddMonths(-1);
                toDate ??= DateTime.UtcNow;

                var transactions = await _transactionRepo.GetByUserIdAsync(userId, cancellationToken);
                var userTransactions = transactions
                    .Where(t => t.TimeStamp >= fromDate && t.TimeStamp <= toDate)
                    .OrderByDescending(t => t.TimeStamp)
                    .ToList();

                var groupedByDate = userTransactions
                    .GroupBy(t => t.TimeStamp.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(g => new ActivityTimelineDto
                    {
                        Date = g.Key,
                        Events = g.Select(t => new ActivityEventDto
                        {
                            Type = "Transaction",
                            Description = t.IsReversed ? $"Transaction reversed: {t.ReversalReason}" : $"{t.Points} points earned",
                            Points = t.Points,
                            TimeStamp = t.TimeStamp
                        }).Cast<ActivityEventDto>().ToList()
                    })
                    .ToList();

                return Ok(groupedByDate);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving activity timeline: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving activity timeline" });
            }
        }

        /// <summary>
        /// Get admin dashboard (Admin only)
        /// </summary>
        [HttpGet("admin/dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting admin dashboard");

                // Get transaction stats
                var today = DateTime.UtcNow.Date;
                var recentTransactions = await _transactionRepo.GetRecentTransactionsAsync(7, 100, cancellationToken);
                var todayCount = recentTransactions.Count(t => t.TimeStamp.Date == today);
                var eventEarningsCount = recentTransactions.Count(t => (int)t.Source == 0);
                var adminAwardsCount = recentTransactions.Count(t => (int)t.Source == 1);
                var refundsCount = recentTransactions.Count(t => (int)t.Source == 2);

                // Get redemption stats
                var pendingCount = await _redemptionRepo.GetRedemptionCountByStatusAsync(0, cancellationToken);
                var approvedCount = await _redemptionRepo.GetRedemptionCountByStatusAsync(1, cancellationToken);
                var rejectedCount = await _redemptionRepo.GetRedemptionCountByStatusAsync(2, cancellationToken);
                var totalRedemptions = pendingCount + approvedCount + rejectedCount;

                // Get top earners
                var topEarners = await _transactionRepo.GetTopEarnersAsync(10, cancellationToken);

                // Get top products
                var topProducts = await _redemptionRepo.GetTopProductsAsync(10, cancellationToken);

                var dashboard = new AdminDashboardDto
                {
                    Transactions = new TransactionDashboardDto
                    {
                        TotalCount = recentTransactions.Count,
                        TodayCount = todayCount,
                        EventEarnings = eventEarningsCount,
                        AdminAwards = adminAwardsCount,
                        Refunds = refundsCount
                    },
                    Redemptions = new RedemptionDashboardDto
                    {
                        TotalCount = totalRedemptions,
                        PendingCount = pendingCount,
                        ApprovedCount = approvedCount,
                        RejectedCount = rejectedCount,
                        ConversionRate = totalRedemptions > 0 ? (approvedCount / (double)totalRedemptions) * 100 : 0
                    },
                    TopEarners = topEarners.Select(x => new TopEarnerDto
                    {
                        UserId = x.UserId,
                        Name = x.UserName,
                        TotalPoints = x.TotalPoints
                    }).ToList(),
                    TopProducts = topProducts.Select(x => new TopRedemptionProductDto
                    {
                        ProductId = x.ProductId,
                        Name = x.ProductName,
                        RedemptionCount = x.RedemptionCount
                    }).ToList(),
                    RecentActivity = recentTransactions.Take(10).Select(t => new RecentActivityDto
                    {
                        Type = "Transaction",
                        Description = t.Reason,
                        UserName = t.User?.Name ?? "Unknown",
                        TimeStamp = t.TimeStamp
                    }).ToList()
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving admin dashboard: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving admin dashboard" });
            }
        }
    }
}
