using Microsoft.Extensions.Logging;
using Project.Application.DTOs.User;

namespace Project.Application.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
