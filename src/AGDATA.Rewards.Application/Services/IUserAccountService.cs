using Project.Application.DTOs.User;

namespace Project.Application.Services
{
    public interface IUserAccountService
    {
        Task<UserAccountDto> CreateAccountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserAccountDto> GetAccountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserAccountDto> UpdatePointsAsync(Guid userId, int points, CancellationToken cancellationToken = default);
    }
}
