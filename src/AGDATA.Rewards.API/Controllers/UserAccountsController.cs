using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Project.Application.DTOs.User;
using Project.Application.Services;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/account")]
    [Authorize]
    public class UserAccountsController : ControllerBase
    {
        private readonly IUserAccountService _accountService;
        private readonly ILogger<UserAccountsController> _logger;

        public UserAccountsController(IUserAccountService accountService, ILogger<UserAccountsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Create user account (User creates own or Admin creates for anyone)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created account</returns>
        [HttpPost]
        public async Task<ActionResult<UserAccountDto>> CreateAccount(
            Guid userId,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var isAdmin = User.IsInRole("ADMIN");

                // Can only create own account or admin creates for anyone
                if (userId != currentUserId && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized account creation: User {UserId} tried to create for {TargetUserId}",
                        currentUserId, userId);
                    return Forbid();
                }

                _logger.LogInformation("Creating account for user: {UserId}", userId);
                var account = await _accountService.CreateAccountAsync(userId, cancellationToken);
                return CreatedAtAction(nameof(GetAccount), new { userId }, account);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("User not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating account: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating account" });
            }
        }

        /// <summary>
        /// Get user account (Admin or user's own account)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User account details</returns>
        [HttpGet]
        public async Task<ActionResult<UserAccountDto>> GetAccount(
            Guid userId,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var isAdmin = User.IsInRole("ADMIN");

                // Can only access own account or admin can access any
                if (userId != currentUserId && !isAdmin)
                {
                    return Forbid();
                }

                var account = await _accountService.GetAccountAsync(userId, cancellationToken);
                return Ok(account);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Account not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving account: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving account" });
            }
        }

        /// <summary>
        /// Update user account points (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Points update details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated account</returns>
        [HttpPut("points")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserAccountDto>> UpdatePoints(
            Guid userId,
            [FromBody] UpdatePointsRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Admin updating points for user {UserId}: {Points}", userId, request.Points);
                var account = await _accountService.UpdatePointsAsync(userId, request.Points, cancellationToken);
                return Ok(account);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Account not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating points: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating points" });
            }
        }
    }
}
