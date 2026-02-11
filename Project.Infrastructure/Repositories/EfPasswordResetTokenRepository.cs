using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Auth;
using Project.Domain.Interfaces;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories
{
    public class EfPasswordResetTokenRepository: IPasswordResetTokenAsyncRepository
    {
        private readonly AppDbContext _db;
        public EfPasswordResetTokenRepository(AppDbContext db)
        {
            _db = db;
		}
        public async Task<PasswordResetToken> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default )
        {
            if(token == null)
            {
                throw new ArgumentNullException("token");
            }
            await _db.PasswordResetTokens.AddAsync(token, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return token;
		}

        public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            var token = await _db.PasswordResetTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

            return token;
		}

        public async Task UpdateAsync(PasswordResetToken newToken, CancellationToken cancellationToken = default)
        {
            var existing = await _db.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Id == newToken.Id, cancellationToken);

            if(existing == null)
            {
                throw new InvalidOperationException("Token not found");
            }

            _db.Entry(existing).CurrentValues.SetValues(newToken);
            await _db.SaveChangesAsync(cancellationToken);
		}

        public async Task MarkUsedAsPreviousToken(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _db.PasswordResetTokens
                .Where(t => t.UserId == userId && t.UsedAt == null)
                .ToListAsync(cancellationToken);

            foreach(var token in tokens)
            {
                token.UsedAt = DateTime.UtcNow;
			}
            await _db.SaveChangesAsync(cancellationToken);
		}
    }
}
