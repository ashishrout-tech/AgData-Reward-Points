using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Application.DTOs.Transaction;
using Project.Domain.Entities;
using Project.Domain.Enums;
using Project.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionAsyncRepository _transactionRepo;
        private readonly IUserAccountAsyncRepository _userAccountRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionAsyncRepository transactionRepo,
            IUserAccountAsyncRepository userAccountRepo,
            IMapper mapper,
            ILogger<TransactionService> logger)
        {
            _transactionRepo = transactionRepo;
            _userAccountRepo = userAccountRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TransactionDto> RecordEventEarningAsync(RecordEventEarningRequest request, Guid adminId, CancellationToken cancellationToken = default)
        {
            if (request.Points <= 0)
                throw new ArgumentException("Points must be greater than 0");

            var transaction = new Transaction(
                request.UserId,
                request.EventId,
                request.EventParticipantId,
                request.Points,
                request.Rank,
                request.Reason);

            await _transactionRepo.AddAsync(transaction, cancellationToken);

            await _userAccountRepo.UpdateAccountAsync(request.UserId, request.Points, cancellationToken);

            _logger.LogInformation($"Event earning recorded: UserId={request.UserId}, Points={request.Points}, EventId={request.EventId}");
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<TransactionDto> AwardPointsAsync(AwardPointsRequest request, Guid adminId, CancellationToken cancellationToken = default)
        {
            if (request.Points <= 0)
                throw new ArgumentException("Points must be greater than 0");

            var transaction = new Transaction(
                request.UserId,
                request.Points,
                request.Reason,
                adminId,
                isAdminAward: true);

            await _transactionRepo.AddAsync(transaction, cancellationToken);

            // Update user points
            await _userAccountRepo.UpdateAccountAsync(request.UserId, request.Points, cancellationToken);

            _logger.LogInformation($"Admin award: UserId={request.UserId}, Points={request.Points}, AdminId={adminId}");
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId, int? source = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            List<Transaction> transactions;

            if (source.HasValue)
                transactions = await _transactionRepo.GetByUserIdAndSourceAsync(userId, source.Value, skip, take, cancellationToken);
            else
            {
                transactions = await _transactionRepo.GetByUserIdAsync(userId, cancellationToken);
                transactions = transactions.Skip(skip).Take(take).ToList();
            }

            return _mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<TransactionDetailDto> GetTransactionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionRepo.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction {id} not found");

            return _mapper.Map<TransactionDetailDto>(transaction);
        }

        public async Task<TransactionDetailDto> ReverseTransactionAsync(Guid id, ReverseTransactionRequest request, Guid adminId, CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionRepo.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction {id} not found");

            transaction.ReverseTransaction(adminId, request.Reason);
            await _transactionRepo.UpdateAsync(transaction, cancellationToken);

            // Refund user points
            await _userAccountRepo.UpdateAccountAsync(transaction.UserId, -transaction.Points, cancellationToken);

            _logger.LogInformation($"Transaction reversed: TransactionId={id}, UserId={transaction.UserId}, Points={transaction.Points}, AdminId={adminId}");
            return _mapper.Map<TransactionDetailDto>(transaction);
        }

        public async Task<EventEarningsReportDto> GetEventEarningsReportAsync(Guid eventId, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepo.GetByEventIdAsync(eventId, skip, take, cancellationToken);
            var totalPoints = await _transactionRepo.GetEventTotalPointsAsync(eventId, cancellationToken);

            var earnings = transactions.Select(t => new EventEarningItemDto
            {
                UserId = t.UserId,
                UserName = t.User?.Name ?? "Unknown",
                Rank = t.Rank ?? 0,
                Points = t.Points,
                Reason = t.Reason,
                TimeStamp = t.TimeStamp
            }).ToList();

            return new EventEarningsReportDto
            {
                EventId = eventId,
                EventTitle = transactions.FirstOrDefault()?.Event?.Title ?? "Unknown Event",
                TotalPointsAwarded = totalPoints,
                TotalRecords = transactions.Count,
                Earnings = earnings
            };
        }

        public async Task<List<TransactionDto>> GetUserEventEarningsAsync(Guid userId, Guid? eventId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepo.GetUserEventEarningsAsync(userId, eventId, skip, take, cancellationToken);
            return _mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<List<TransactionDto>> GetUserAdminAwardsAsync(Guid userId, Guid? adminId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepo.GetUserAdminAwardsAsync(userId, adminId, skip, take, cancellationToken);
            return _mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<List<ReversedTransactionReportDto>> GetReversedTransactionsReportAsync(Guid? userId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepo.GetReversedTransactionsAsync(userId, skip, take, cancellationToken);

            return transactions.Select(t => new ReversedTransactionReportDto
            {
                Id = t.Id,
                UserId = t.UserId,
                UserName = t.User?.Name ?? "Unknown",
                OriginalPoints = t.Points,
                OriginalReason = t.Reason,
                ReversalReason = t.ReversalReason ?? "",
                ReversedBy = t.ReversedBy ?? Guid.Empty,
                ReversedOn = t.ReversedOn ?? DateTime.UtcNow
            }).ToList();
        }
    }
}
