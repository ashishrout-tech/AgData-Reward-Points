using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Application.DTOs.User;
using Project.Domain.Entities.Users;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IUserAccountAsyncRepository _accountRepository;
        private readonly IUserAsyncRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserAccountService> _logger;

        public UserAccountService(
            IUserAccountAsyncRepository accountRepository,
            IUserAsyncRepository userRepository,
            IMapper mapper,
            ILogger<UserAccountService> logger)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserAccountDto> CreateAccountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating account for user: {UserId}", userId);

                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID '{userId}' not found");
                }

                var existingAccount = await _accountRepository.GetAccountByUserIdAsync(userId, cancellationToken);
                if (existingAccount != null)
                {
                    throw new InvalidOperationException($"Account already exists for user {userId}");
                }

                var account = new UserAccount(userId);
                var createdAccount = await _accountRepository.AddAsync(account, cancellationToken);

                _logger.LogInformation("Account created successfully for user: {UserId}", userId);

                return _mapper.Map<UserAccountDto>(createdAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating account: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<UserAccountDto> GetAccountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting account for user: {UserId}", userId);

            var account = await _accountRepository.GetAccountByUserIdAsync(userId, cancellationToken);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account not found for user {userId}");
            }

            return _mapper.Map<UserAccountDto>(account);
        }

        public async Task<UserAccountDto> UpdatePointsAsync(Guid userId, int points, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating points for user: {UserId}, New Points: {Points}", userId, points);

                var account = await _accountRepository.GetAccountByUserIdAsync(userId, cancellationToken);
                if (account == null)
                {
                    throw new KeyNotFoundException($"Account not found for user {userId}");
                }

                await _accountRepository.UpdateAccountAsync(userId, points, cancellationToken);

                var updatedAccount = await _accountRepository.GetAccountByUserIdAsync(userId, cancellationToken);

                _logger.LogInformation("Points updated successfully for user: {UserId}", userId);

                return _mapper.Map<UserAccountDto>(updatedAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating points: {Message}", ex.Message);
                throw;
            }
        }
    }
}
