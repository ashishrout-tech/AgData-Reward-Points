using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Product;
using Project.Domain.Interfaces;
using Project.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class EfProductRepository : IProductAsyncRepository
    {
        private readonly AppDbContext _db;

        public EfProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _db.Products.AddAsync(product, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return product;
        }

        public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Products
                .AsNoTracking()
                .Include(p => p.ProductPrice)
                .Include(p => p.ProductStock)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Products
                .AsNoTracking()
                .Include(p => p.ProductPrice)
                .Include(p => p.ProductStock)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _db.Products.FindAsync(new object[] { id }, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            _db.Products.Remove(product);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

            if (existing == null)
                throw new KeyNotFoundException("Product not found.");

            _db.Entry(existing).CurrentValues.SetValues(product);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Product>> SearchAsync(string? name, string? brand, CancellationToken cancellationToken = default)
        {
            var query = _db.Products.AsNoTracking();

            query = query.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand.Contains(brand, StringComparison.OrdinalIgnoreCase));
            }

            return await query
                .Include(p => p.ProductPrice)
                .Include(p => p.ProductStock)
                .ToListAsync(cancellationToken);
        }
    }
}
