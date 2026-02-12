using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Application.DTOs.User;
using Project.Domain.Entities.Users;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserAsyncRepository _userRepository;
        private readonly IProfilePictureGeneratorService _profilePictureGeneratorService;
		private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserAsyncRepository userRepository,
            IProfilePictureGeneratorService profilePictureGeneratorService,
			IMapper mapper,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _profilePictureGeneratorService = profilePictureGeneratorService;
			_mapper = mapper;
            _logger = logger;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating user: {Email}", request.Email);

                var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email '{request.Email}' already exists");
                }

                var existingUserByEmpId = await _userRepository.GetByEmployeeIdAsync(request.EmployeeId, cancellationToken);
                if (existingUserByEmpId != null)
                {
                    throw new InvalidOperationException($"User with employee ID '{request.EmployeeId}' already exists");
                }

                var user = new User(request.Name, request.Email, request.EmployeeId, request.Role, request.Password);

                user.CreateUserAccount();

                if(request.PhotoId.HasValue)
                {
                    user.SetPhoto(request.PhotoId.Value);
				}
                else
                {
                    var id = await _profilePictureGeneratorService.GenerateProfilePictureAsync(request.Name, cancellationToken);
                    user.SetPhoto(id);
				}

                var createdUser = await _userRepository.AddAsync(user, cancellationToken);

                _logger.LogInformation("User created successfully: {UserId}", createdUser.Id);

                return _mapper.Map<UserDto>(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<UserDto> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found");
            }

            return _mapper.Map<UserDto>(user);
        }

        public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all users");

            var users = await _userRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating user: {UserId}", id);

                var user = await _userRepository.GetByIdAsync(id, cancellationToken);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID '{id}' not found");
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    var nameProperty = typeof(User).GetProperty("Name");
                    nameProperty?.SetValue(user, request.Name);
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                    if (existingUser != null && existingUser.Id != id)
                    {
                        throw new InvalidOperationException($"Email '{request.Email}' is already in use");
                    }

                    var emailProperty = typeof(User).GetProperty("Email");
                    emailProperty?.SetValue(user, request.Email);
                }

                if (request.Role.HasValue)
                {
                    user.UpdateUserRole(request.Role.Value);
                }

                if (request.PhotoId.HasValue)
                {
                    user.SetPhoto(request.PhotoId.Value);
                }

                if (request.IsActive.HasValue)
                {
                    if (request.IsActive.Value)
                    {
                        user.ActivateUser();
                    }
                    else
                    {
                        user.DeactivateUser();
                    }
                }

                if(user.PhotoId == null)
                {
                    var generatedPhotoId = await _profilePictureGeneratorService.GenerateProfilePictureAsync(user.Name, cancellationToken);
                    user.SetPhoto(generatedPhotoId);
                }

                await _userRepository.UpdateAsync(user, cancellationToken);

                _logger.LogInformation("User updated successfully: {UserId}", id);

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deactivating user: {UserId}", id);

                var user = await _userRepository.GetByIdAsync(id, cancellationToken);
                if (user == null)
                {
                    return false;
                }

                user.DeactivateUser();
                await _userRepository.UpdateAsync(user, cancellationToken);

                _logger.LogInformation("User deactivated successfully: {UserId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deactivating user: {Message}", ex.Message);
                throw;
            }
        }
    }
}
