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
    public class EfUserRepository : IUserAsyncRepository
    {
        private readonly AppDbContext _db;

        public EfUserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            
            bool exists = await _db.Users
                .AnyAsync(u => u.Email == user.Email || u.EmployeeId == user.EmployeeId, cancellationToken);

            if (exists)
                throw new InvalidOperationException($"User with email '{user.Email}' or employee id '{user.EmployeeId}' already exists.");

            await _db.Users.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return user;
        }

        public async Task<User?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Users.FindAsync(new object[] { user.Id }, cancellationToken);
            if (existing == null)
                throw new KeyNotFoundException("User not found");

            _db.Entry(existing).CurrentValues.SetValues(user);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
