using Project.Application.DTOs.Transaction;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public interface ITransactionService
    {
        Task<TransactionDto> RecordEventEarningAsync(RecordEventEarningRequest request, Guid adminId, CancellationToken cancellationToken = default);
        Task<TransactionDto> AwardPointsAsync(AwardPointsRequest request, Guid adminId, CancellationToken cancellationToken = default);
        Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId, int? source = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<TransactionDetailDto> GetTransactionAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TransactionDetailDto> ReverseTransactionAsync(Guid id, ReverseTransactionRequest request, Guid adminId, CancellationToken cancellationToken = default);
        Task<EventEarningsReportDto> GetEventEarningsReportAsync(Guid eventId, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<TransactionDto>> GetUserEventEarningsAsync(Guid userId, Guid? eventId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<TransactionDto>> GetUserAdminAwardsAsync(Guid userId, Guid? adminId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<ReversedTransactionReportDto>> GetReversedTransactionsReportAsync(Guid? userId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
    }
}
