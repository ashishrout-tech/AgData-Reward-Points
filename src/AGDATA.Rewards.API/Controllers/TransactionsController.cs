using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Transaction;
using Project.Application.Services;
using System.Security.Claims;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
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
        /// Record event-based point earning (Admin only)
        /// </summary>
        [HttpPost("event-earning")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<TransactionDto>> RecordEventEarning(
            [FromBody] RecordEventEarningRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Recording event earning: UserId={UserId}, EventId={EventId}", request.UserId, request.EventId);
                var adminId = GetCurrentUserId();
                var transaction = await _transactionService.RecordEventEarningAsync(request, adminId, cancellationToken);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error recording event earning: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while recording event earning" });
            }
        }

        /// <summary>
        /// Award points by admin (Admin only)
        /// </summary>
        [HttpPost("admin-award")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<TransactionDto>> AwardPoints(
            [FromBody] AwardPointsRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Awarding points: UserId={UserId}, Points={Points}", request.UserId, request.Points);
                var adminId = GetCurrentUserId();
                var transaction = await _transactionService.AwardPointsAsync(request, adminId, cancellationToken);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error awarding points: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while awarding points" });
            }
        }

        /// <summary>
        /// Get user transaction history
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<TransactionDto>>> GetUserTransactions(
            Guid userId,
            [FromQuery] int? source = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId && !IsAdmin())
                    return Forbid();

                _logger.LogInformation("Getting user transactions: UserId={UserId}", userId);
                var transactions = await _transactionService.GetUserTransactionsAsync(userId, source, skip, take, cancellationToken);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving transactions: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving transactions" });
            }
        }

        /// <summary>
        /// Get transaction details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetailDto>> GetTransaction(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting transaction: {TransactionId}", id);
                var transaction = await _transactionService.GetTransactionAsync(id, cancellationToken);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Transaction not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving transaction: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving transaction" });
            }
        }

        /// <summary>
        /// Reverse transaction (Admin only)
        /// </summary>
        [HttpPost("{id}/reverse")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<TransactionDetailDto>> ReverseTransaction(
            Guid id,
            [FromBody] ReverseTransactionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _logger.LogInformation("Reversing transaction: {TransactionId}", id);
                var adminId = GetCurrentUserId();
                var transaction = await _transactionService.ReverseTransactionAsync(id, request, adminId, cancellationToken);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Transaction not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error reversing transaction: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while reversing transaction" });
            }
        }

        /// <summary>
        /// Get event earnings report (Admin only)
        /// </summary>
        [HttpGet("reports/event/{eventId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<EventEarningsReportDto>> GetEventEarningsReport(
            Guid eventId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting event earnings report: {EventId}", eventId);
                var report = await _transactionService.GetEventEarningsReportAsync(eventId, skip, take, cancellationToken);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving event report: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving event report" });
            }
        }

        /// <summary>
        /// Get user's event earnings
        /// </summary>
        [HttpGet("user/{userId}/event-earnings")]
        public async Task<ActionResult<List<TransactionDto>>> GetUserEventEarnings(
            Guid userId,
            [FromQuery] Guid? eventId = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId && !IsAdmin())
                    return Forbid();

                _logger.LogInformation("Getting event earnings for user: {UserId}", userId);
                var transactions = await _transactionService.GetUserEventEarningsAsync(userId, eventId, skip, take, cancellationToken);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving event earnings: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving event earnings" });
            }
        }

        /// <summary>
        /// Get user's admin awards
        /// </summary>
        [HttpGet("user/{userId}/admin-awards")]
        public async Task<ActionResult<List<TransactionDto>>> GetUserAdminAwards(
            Guid userId,
            [FromQuery] Guid? adminId = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId && !IsAdmin())
                    return Forbid();

                _logger.LogInformation("Getting admin awards for user: {UserId}", userId);
                var transactions = await _transactionService.GetUserAdminAwardsAsync(userId, adminId, skip, take, cancellationToken);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving admin awards: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving admin awards" });
            }
        }

        /// <summary>
        /// Get reversed transactions report (Admin only)
        /// </summary>
        [HttpGet("reports/reversed")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<List<ReversedTransactionReportDto>>> GetReversedTransactionsReport(
            [FromQuery] Guid? userId = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting reversed transactions report");
                var report = await _transactionService.GetReversedTransactionsReportAsync(userId, skip, take, cancellationToken);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving reversed transactions report: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving reversed transactions report" });
            }
        }
    }
}
