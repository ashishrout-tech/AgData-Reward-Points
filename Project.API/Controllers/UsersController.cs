using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Project.Application.DTOs.User;
using Project.Application.Services;
using Project.Domain.Enums;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all users</returns>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Admin retrieving all users");
                var users = await _userService.GetAllUsersAsync(cancellationToken);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving users: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Get user by ID (Admin or user's own profile)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("ADMIN");

                if (id.ToString() != currentUserId && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access user {TargetUserId}",
                        currentUserId, id);
                    return Forbid();
                }

                var user = await _userService.GetUserAsync(id, cancellationToken);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("User not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving user: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving user" });
            }
        }

        /// <summary>
        /// Register new user account (Public - anyone can register)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> RegisterUser(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("New user registering: {Email}", request.Email);
                var user = await _userService.CreateUserAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error registering user: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while registering user" });
            }
        }

        /// <summary>
        /// Create user (Admin only)
        /// </summary>
        /// <param name="request">User creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created user</returns>
        [HttpPost("admin/create")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserDto>> CreateUserAsAdmin(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Admin creating new user: {Email}", request.Email);
                var user = await _userService.CreateUserAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating user: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating user" });
            }
        }

        /// <summary>
        /// Update user (Admin can update any user, Employees can update themselves)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Update details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated user</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(
            Guid id,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var isAdmin = User.IsInRole("ADMIN");

                // Employee cannot update other users
                if (!isAdmin && id != currentUserId)
                {
                    _logger.LogWarning("Unauthorized update attempt: User {UserId} tried to update user {TargetUserId}",
                        currentUserId, id);
                    return Forbid();
                }

                // Employee cannot change their own role
                if (!isAdmin && request.Role != null)
                {
                    return BadRequest(new { message = "Employees cannot change their role" });
                }

                _logger.LogInformation("User {UserId} updating user {TargetUserId}", currentUserId, id);
                var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
                return Ok(user);
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
                _logger.LogError("Error updating user: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating user" });
            }
        }

        /// <summary>
        /// Deactivate user (Admin only) - Soft delete
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Admin deactivating user: {UserId}", id);
                var success = await _userService.DeactivateUserAsync(id, cancellationToken);

                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deactivating user: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while deactivating user" });
            }
        }
    }
}
