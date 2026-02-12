using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Domain.Entities.Auth;

namespace Project.Domain.Interfaces
{
    public interface IPasswordResetTokenAsyncRepository
    {
        Task<PasswordResetToken> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
        Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task UpdateAsync(PasswordResetToken resetToken, CancellationToken cancellationToken = default);
        Task MarkUsedAsPreviousToken(Guid userId, CancellationToken cancellationToken = default);
	}
}
