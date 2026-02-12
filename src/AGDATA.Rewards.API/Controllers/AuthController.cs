using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Project.Application.DTOs.User;
using Project.Application.Services;
using Project.Application.DTOs.Auth;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);
                var response = await _authService.LoginAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed login attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during login: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<PasswordResetResponse>> SendPasswordResetMail(
            [FromBody] ForgotPasswordRequest request,
			CancellationToken cancellationToken = default)
        {
            try
            {
                await _authService.SendPasswordResetMailAsync(request.Email, cancellationToken);
                return Ok(new PasswordResetResponse
                {
                    Success = true,
					Message = "If an account exists with this email, a reset link has been sent"
				});
			}
            catch (Exception ex)
            {
                _logger.LogError("Error sending password reset mail: {Message}", ex.Message);
                return Ok(new PasswordResetResponse
                {
                    Success = true,
                    Message = "If an account exists with this email, a reset link has been sent"
                });
			}
		}

        [HttpPost("reset-password")]
        [AllowAnonymous]
		public async Task<ActionResult<PasswordResetResponse>> ResetPassword(
            [FromQuery] string token,
            [FromBody] PasswordResetRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _authService.ResetPasswordAsync(token, request.NewPassword, cancellationToken);
                return Ok(new PasswordResetResponse
                {
                    Success = true,
                    Message = "Password has been reset successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Invalid password reset token: {Message}", ex.Message);
                return Unauthorized(new PasswordResetResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error resetting password: {Message}", ex.Message);
                return StatusCode(500, new PasswordResetResponse
                {
                    Success = false,
                    Message = "An error occurred while resetting the password"
                });
			}
		}
    }
}
