using System;
using System.Collections.Generic;
using Project.Domain.Entities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface ITransactionAsyncRepository
    {
        Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
        
        // Filtering methods
        Task<List<Transaction>> GetByUserIdAndSourceAsync(Guid userId, int source, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetByEventIdAsync(Guid eventId, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetReversedTransactionsAsync(Guid? userId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        
        // Statistics methods
        Task<int> GetEventTotalPointsAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetUserEventEarningsAsync(Guid userId, Guid? eventId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetUserAdminAwardsAsync(Guid userId, Guid? adminId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<int> GetTotalPointsEarnedAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> GetTotalPointsRedeemedAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetRecentTransactionsAsync(int? days, int take = 100, CancellationToken cancellationToken = default);
        Task<List<(Guid UserId, Guid? PhotoId, string UserName, int TotalPoints)>> GetTopEarnersAsync(int take = 10, CancellationToken cancellationToken = default);
    }
}
