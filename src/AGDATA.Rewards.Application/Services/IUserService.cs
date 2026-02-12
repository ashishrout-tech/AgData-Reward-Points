using Project.Application.DTOs.User;

namespace Project.Application.Services
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<UserDto> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeactivateUserAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
