using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IRedemptionAsyncRepository
    {
        Task<Redemption> AddAsync(Redemption redemption, CancellationToken cancellationToken = default);
        Task<Redemption?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Redemption>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<Redemption>> GetPendingByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task UpdateAsync(Redemption redemption, CancellationToken cancellationToken = default);
        
        // Additional filtering and reporting methods
        Task<List<Redemption>> GetPendingAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<Redemption>> GetApprovedAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<Redemption>> GetRejectedAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
		Task<List<Redemption>> GetByStatusAsync(int status, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<Redemption>> GetUserRedemptionsByStatusAsync(Guid userId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<Redemption>> GetRedemptionsByProductAsync(Guid productId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<int> GetTotalPointsRedeemedAsync(CancellationToken cancellationToken = default);
        Task<int> GetTotalPointsRefundedAsync(CancellationToken cancellationToken = default);
        Task<List<Redemption>> GetRejectedRedemptionsAsync(string? reason = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<int> GetRedemptionCountByStatusAsync(int status, Guid? userId, CancellationToken cancellationToken = default);
        Task<List<(Guid ProductId, Guid? PhotoId, string ProductName, int RedemptionCount)>> GetTopProductsAsync(int take = 10, CancellationToken cancellationToken = default);
    }
}
