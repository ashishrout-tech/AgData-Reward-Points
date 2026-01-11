using Project.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IUserAccountAsyncRepository
    {
        Task<UserAccount> AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default);
        Task UpdateAccountAsync(Guid userId, int points, CancellationToken cancellationToken = default);
        Task<UserAccount?> GetAccountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
