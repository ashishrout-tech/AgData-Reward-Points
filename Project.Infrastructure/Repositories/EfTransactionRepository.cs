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
    public class EfTransactionRepository : ITransactionAsyncRepository
    {
        private readonly AppDbContext _db;

        public EfTransactionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            await _db.Transactions.AddAsync(transaction, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return transaction;
        }

        public async Task<List<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TimeStamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            _db.Transactions.Update(transaction);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Transaction>> GetByUserIdAndSourceAsync(Guid userId, int source, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && (int)t.Source == source)
                .OrderByDescending(t => t.TimeStamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Transaction>> GetByEventIdAsync(Guid eventId, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Where(t => t.EventId == eventId && !t.IsReversed)
                .OrderByDescending(t => t.Rank)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Transaction>> GetReversedTransactionsAsync(Guid? userId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Where(t => t.IsReversed);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            return await query
                .OrderByDescending(t => t.ReversedOn)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetEventTotalPointsAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.EventId == eventId && !t.IsReversed)
                .SumAsync(t => t.Points, cancellationToken);
        }

        public async Task<List<Transaction>> GetUserEventEarningsAsync(Guid userId, Guid? eventId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && (int)t.Source == 0 && !t.IsReversed); // Source 0 = Event

            if (eventId.HasValue)
                query = query.Where(t => t.EventId == eventId.Value);

            return await query
                .OrderByDescending(t => t.TimeStamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Transaction>> GetUserAdminAwardsAsync(Guid userId, Guid? adminId = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var query = _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && (int)t.Source == 1 && !t.IsReversed); // Source 1 = AdminAward

            if (adminId.HasValue)
                query = query.Where(t => t.AdminApprovedBy == adminId.Value);

            return await query
                .OrderByDescending(t => t.TimeStamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalPointsEarnedAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && !t.IsReversed)
                .SumAsync(t => t.Points, cancellationToken);
        }

        public async Task<int> GetTotalPointsRedeemedAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && (int)t.Source == 3 && !t.IsReversed) // Source 3 = Redeem
                .SumAsync(t => t.Points, cancellationToken);
        }

        public async Task<List<Transaction>> GetRecentTransactionsAsync(int days = 7, int take = 100, CancellationToken cancellationToken = default)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            return await _db.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Where(t => t.TimeStamp >= fromDate)
                .OrderByDescending(t => t.TimeStamp)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<(Guid UserId, string UserName, int TotalPoints)>> GetTopEarnersAsync(int take = 10, CancellationToken cancellationToken = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Where(t => !t.IsReversed)
                .GroupBy(t => new { t.UserId, t.User.Name })
                .Select(g => new ValueTuple<Guid, string, int>(g.Key.UserId, g.Key.Name, g.Sum(t => t.Points)))
                .OrderByDescending(x => x.Item3)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
    }
}
