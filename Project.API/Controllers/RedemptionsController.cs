using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Redemption;
using Project.Application.Services;
using System.Security.Claims;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RedemptionsController : ControllerBase
    {
        private readonly IRedemptionService _redemptionService;
        private readonly ILogger<RedemptionsController> _logger;

        public RedemptionsController(IRedemptionService redemptionService, ILogger<RedemptionsController> logger)
        {
            _redemptionService = redemptionService;
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
            return roleClaim?.Value == "ADMIN";
        }

        /// <summary>
        /// Create redemption request
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RedemptionDetailDto>> CreateRedemption(
            [FromBody] CreateRedemptionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                _logger.LogInformation("Creating redemption: UserId={UserId}, ProductId={ProductId}", userId, request.ProductId);
                var redemption = await _redemptionService.CreateRedemptionAsync(userId, request, cancellationToken);
                return CreatedAtAction(nameof(GetRedemption), new { id = redemption.Id }, redemption);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating redemption: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating redemption" });
            }
        }

        /// <summary>
        /// Get pending redemptions (Admin only)
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<List<RedemptionPendingDto>>> GetPendingRedemptions(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting pending redemptions");
                var redemptions = await _redemptionService.GetPendingRedemptionsAsync(skip, take, cancellationToken);
                return Ok(redemptions);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving pending redemptions: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving pending redemptions" });
            }
        }

		/// <summary>
		/// Get pending redemptions (Admin only)
		/// </summary>
		[HttpGet("approved")]
		[Authorize(Roles = "ADMIN")]
		public async Task<ActionResult<List<RedemptionApprovedDto>>> GetApprovedRedemptions(
			[FromQuery] int skip = 0,
			[FromQuery] int take = 10,
			CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogInformation("Getting pending redemptions");
				var redemptions = await _redemptionService.GetApprovedRedemptionsAsync(skip, take, cancellationToken);
				return Ok(redemptions);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error retrieving pending redemptions: {Message}", ex.Message);
				return StatusCode(500, new { message = "An error occurred while retrieving pending redemptions" });
			}
		}

		/// <summary>
		/// Get pending redemptions (Admin only)
		/// </summary>
		[HttpGet("rejected")]
		[Authorize(Roles = "ADMIN")]
		public async Task<ActionResult<List<RedemptionRejectedDto>>> GetRejectedRedemptions(
			[FromQuery] int skip = 0,
			[FromQuery] int take = 10,
			CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogInformation("Getting pending redemptions");
				var redemptions = await _redemptionService.GetRejectedRedemptionsAsync(skip, take, cancellationToken);
				return Ok(redemptions);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error retrieving pending redemptions: {Message}", ex.Message);
				return StatusCode(500, new { message = "An error occurred while retrieving pending redemptions" });
			}
		}

		/// <summary>
		/// Get user redemption history
		/// </summary>
		[HttpGet("user/{userId}")]
        public async Task<ActionResult<List<RedemptionDto>>> GetUserRedemptions(
            Guid userId,
            [FromQuery] int? status = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId && !IsAdmin())
                    return Forbid();

                _logger.LogInformation("Getting redemptions for user: {UserId}", userId);
                var redemptions = await _redemptionService.GetUserRedemptionsAsync(userId, status, skip, take, cancellationToken);
                return Ok(redemptions);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving user redemptions: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving redemptions" });
            }
        }

        /// <summary>
        /// Get redemption details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RedemptionDetailDto>> GetRedemption(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting redemption: {RedemptionId}", id);
                var redemption = await _redemptionService.GetRedemptionAsync(id, cancellationToken);
                return Ok(redemption);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Redemption not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving redemption: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving redemption" });
            }
        }

        /// <summary>
        /// Approve redemption (Admin only)
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<RedemptionApproveResponseDto>> ApproveRedemption(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Approving redemption: {RedemptionId}", id);
                var adminId = GetCurrentUserId();
                var redemption = await _redemptionService.ApproveRedemptionAsync(id, adminId, cancellationToken);
                return Ok(redemption);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Redemption not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error approving redemption: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while approving redemption" });
            }
        }

        /// <summary>
        /// Reject redemption (Admin only)
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<RedemptionRejectResponseDto>> RejectRedemption(
            Guid id,
            [FromBody] RejectRedemptionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Rejecting redemption: {RedemptionId}", id);
                var adminId = GetCurrentUserId();
                var redemption = await _redemptionService.RejectRedemptionAsync(id, request, adminId, cancellationToken);
                return Ok(redemption);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Redemption not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error rejecting redemption: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while rejecting redemption" });
            }
        }

        /// <summary>
        /// Get redemptions by product (Admin only)
        /// </summary>
        [HttpGet("product/{productId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<List<RedemptionDto>>> GetRedemptionsByProduct(
            Guid productId,
            [FromQuery] int? status = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting redemptions for product: {ProductId}", productId);
                var redemptions = await _redemptionService.GetRedemptionsByProductAsync(productId, status, skip, take, cancellationToken);
                return Ok(redemptions);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving product redemptions: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving product redemptions" });
            }
        }

        /// <summary>
        /// Get redemption statistics (Admin only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<RedemptionStatisticsDto>> GetStatistics(
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting redemption statistics");
                var statistics = await _redemptionService.GetStatisticsAsync(cancellationToken);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving statistics: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Get rejected redemptions report (Admin only)
        /// </summary>
        [HttpGet("reports/rejected")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<List<RejectedRedemptionReportDto>>> GetRejectedRedemptionsReport(
            [FromQuery] string? reason = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting rejected redemptions report");
                var report = await _redemptionService.GetRejectedRedemptionsReportAsync(reason, skip, take, cancellationToken);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving rejected report: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving rejected report" });
            }
        }
    }
}
