using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Users;
using Project.Domain.Interfaces;
using Project.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class EfUserAccountRepository : IUserAccountAsyncRepository
    {
        private readonly AppDbContext _db;

        public EfUserAccountRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserAccount> AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
        {
          
            bool exists = await _db.UserAccounts
                .AsNoTracking()
                .AnyAsync(a => a.UserId == userAccount.UserId, cancellationToken);

            if (exists)
                throw new InvalidOperationException($"Account for user {userAccount.UserId} already exists.");

            await _db.UserAccounts.AddAsync(userAccount, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return userAccount;
        }

        public async Task UpdateAccountAsync(Guid userId, int points, CancellationToken cancellationToken = default)
        {
            var account = await _db.UserAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

            if (account == null)
                throw new KeyNotFoundException("User account not found.");

            account.AddPoints(points);

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<UserAccount?> GetAccountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            
            return await _db.UserAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
        }
    }
}
