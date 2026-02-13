using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Project.Application.DTOs.User;
using Project.Domain.Interfaces;
using Project.Domain.Entities.Auth;
using AGDATA.Rewards.Core.UserAggregate;

namespace Project.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserAsyncRepository _userRepository;
        private readonly IPasswordResetTokenAsyncRepository _passwordResetTokenRepository;
        private readonly IEmailService _emailService;
		private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserAsyncRepository userRepository,
            IPasswordResetTokenAsyncRepository passwordResetTokenRepository,
            IEmailService emailService,
			IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
			_configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null || !user.VerifyPassword(request.Password))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
                throw new UnauthorizedAccessException("User account is inactive");
            }

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpiresInHours"] ?? "1"));

            return new LoginResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role == 0 ? "EMPLOYEE" : "ADMIN",
                AccessToken = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<bool> SendPasswordResetMailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
				var user = await _userRepository.GetByEmailAsync(email);
				if (user == null || !user.IsActive)
				{
					_logger.LogWarning("Password reset requested for non-existent or inactive email: {Email}", email);
					return false;
				}
                await _passwordResetTokenRepository.MarkUsedAsPreviousToken(user.Id, cancellationToken);

				var resetToken = await GeneratePasswordResetToken(user, cancellationToken);
				var resetLink = $"{_configuration["Frontend:Url"]}/reset-link?token={resetToken.TokenHash}";
				string mailSubject = "Please reset the password";
				string mailBody = $"The reset link: {resetLink}";
				await _emailService.SendAsync(email, mailSubject, mailBody);
				return true;
			}
            catch (Exception ex)
            {
                _logger.LogError("Error sending mail: {Message}", ex.Message);
                throw;
            }
		}

        public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
        {
			var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(token);
			if (resetToken == null)
			{
				_logger.LogWarning("Invalid password reset token attempted");
				throw new UnauthorizedAccessException("Invalid or expired reset token");
			}

			if (resetToken.UsedAt != null)
			{
				_logger.LogWarning("Attempted to reuse password reset token: {TokenId}", resetToken.Id);
				throw new UnauthorizedAccessException("This reset link has already been used");
			}

			if (resetToken.ExpiresAt < DateTime.UtcNow)
			{
				_logger.LogWarning("Expired password reset token attempted: {TokenId}", resetToken.Id);
				throw new UnauthorizedAccessException("This reset link has expired");
			}

			var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
			if (user == null || !user.IsActive)
			{
				throw new UnauthorizedAccessException("User not found or inactive");
			}

			user.ChangePassword(null, newPassword);
			await _userRepository.UpdateAsync(user, cancellationToken);

			resetToken.UsedAt = DateTime.UtcNow;
			await _passwordResetTokenRepository.UpdateAsync(resetToken, cancellationToken);
			_logger.LogInformation("Password reset successful for user: {UserId}", user.Id);

			return true;
		}

        private async Task<PasswordResetToken> GeneratePasswordResetToken(User user, CancellationToken cancellationToken = default)
        {
            try
            {
				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["ResetJwt:Secret"]!));
				var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

				var claims = new[]
				{
				    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				    new Claim(ClaimTypes.Email, user.Email)
			    };

				var tokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["ResetJwt:ExpiresInMinutes"] ?? "30"));


				var token = new JwtSecurityToken(
					issuer: _configuration["ResetJwt:Issuer"],
					audience: _configuration["ResetJwt:Audience"],
					claims: claims,
					expires: tokenExpiry,
					signingCredentials: credentials);

				var generatedTokenHash = new JwtSecurityTokenHandler().WriteToken(token);
				PasswordResetToken passwordResetToken = new PasswordResetToken(user.Id, generatedTokenHash, tokenExpiry);

				var createdToken = await _passwordResetTokenRepository.AddAsync(passwordResetToken, cancellationToken);
				return createdToken;
			}
            catch (Exception ex)
            {
                _logger.LogError("Error generating password reset token: {Message}", ex.Message);
                throw;
			}
		}

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpiresInHours"] ?? "1")),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
