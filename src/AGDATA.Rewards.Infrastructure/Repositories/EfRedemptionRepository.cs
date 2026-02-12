using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class EfRedemptionRepository : IRedemptionAsyncRepository
    {
        private readonly AppDbContext _db;

        public EfRedemptionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Redemption> AddAsync(Redemption redemption, CancellationToken cancellationToken = default)
        {
            await _db.Redemptions.AddAsync(redemption, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);

			return redemption;
		}

        public async Task<Redemption?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
                .Include(r => r.ApprovalAdmin)
                .Include(r => r.RejectionAdmin)
				.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<List<Redemption>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
				.Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
				.Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Redemption>> GetPendingByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && (int)r.Status == 0) // Status 0 = Pending
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Redemption redemption, CancellationToken cancellationToken = default)
        {
            _db.Redemptions.Update(redemption);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Redemption>> GetPendingAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            return await _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .ThenInclude(p => p.ProductPrice)
                .Where(r => (int)r.Status == 0) // Status 0 = Pending
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

		public async Task<List<Redemption>> GetApprovedAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default)
		{
			return await _db.Redemptions
				.AsNoTracking()
				.Include(r => r.User)
				.Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
                .Include(r => r.ApprovalAdmin)
				.Where(r => (int)r.Status == 1)
				.OrderByDescending(r => r.CreatedAt)
				.Skip(skip)
				.Take(take)
				.ToListAsync(cancellationToken);
		}

		public async Task<List<Redemption>> GetRejectedAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default)
		{
			return await _db.Redemptions
				.AsNoTracking()
				.Include(r => r.User)
				.Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
                .Include(r => r.RejectionAdmin)
				.Where(r => (int)r.Status == 2)
				.OrderByDescending(r => r.CreatedAt)
				.Skip(skip)
				.Take(take)
				.ToListAsync(cancellationToken);
		}

		public async Task<List<Redemption>> GetByStatusAsync(int status, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            return await _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
				.Where(r => (int)r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Redemption>> GetUserRedemptionsByStatusAsync(Guid userId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
				.Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
				.Where(r => r.UserId == userId);

            if (status.HasValue)
                query = query.Where(r => (int)r.Status == status.Value);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Redemption>> GetRedemptionsByProductAsync(Guid productId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .ThenInclude(p => p.ProductPrice)
				.Where(r => r.ProductId == productId);

            if (status.HasValue)
                query = query.Where(r => (int)r.Status == status.Value);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalPointsRedeemedAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Redemptions
                .AsNoTracking()
                .Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
				.Where(r => (int)r.Status == 1) // Status 1 = Approved
                .SumAsync(r => (int?)r.Product.ProductPrice.CurrentPoints ?? 0, cancellationToken);
        }

        public async Task<int> GetTotalPointsRefundedAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Redemptions
                .AsNoTracking()
                .Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
				.Where(r => (int)r.Status == 2) // Status 2 = Rejected
                .SumAsync(r => (int?)r.Product.ProductPrice.CurrentPoints ?? 0, cancellationToken);
        }

        public async Task<List<Redemption>> GetRejectedRedemptionsAsync(string? reason = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Redemptions
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
				.ThenInclude(p => p.ProductPrice)
				.Where(r => (int)r.Status == 2); // Status 2 = Rejected

            if (!string.IsNullOrEmpty(reason))
                query = query.Where(r => r.RejectionReason!.Contains(reason));

            return await query
                .OrderByDescending(r => r.RejectedOn)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetRedemptionCountByStatusAsync(int status, Guid? userId, CancellationToken cancellationToken = default)
        {
            if(userId != null)
            {
                return await _db.Redemptions
                    .AsNoTracking()
                    .Where(r => r.UserId == userId && (int)r.Status == status)
                    .CountAsync(cancellationToken);
			}
			return await _db.Redemptions
                .AsNoTracking()
                .Where(r => (int)r.Status == status)
                .CountAsync(cancellationToken);
        }

        public async Task<List<(Guid ProductId, Guid? PhotoId, string ProductName, int RedemptionCount)>> GetTopProductsAsync(int take = 10, CancellationToken cancellationToken = default)
        {
			var redemptions = await _db.Redemptions
		    .AsNoTracking()
		    .Include(r => r.Product)
		    .Where(r => (int)r.Status == 1) // Status 1 = Approved
		    .ToListAsync(cancellationToken);

			return redemptions
				.GroupBy(r => new { r.ProductId, r.Product.PhotoId, r.Product.Name })
				.Select(g => (g.Key.ProductId, g.Key.PhotoId, g.Key.Name, g.Count()))
				.OrderByDescending(x => x.Item3)
				.Take(take)
				.ToList();
		}
    }
}
