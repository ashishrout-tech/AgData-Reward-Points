using Microsoft.Extensions.Logging;
using Project.Application.DTOs.User;

namespace Project.Application.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<bool> SendPasswordResetMailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
	}
}
