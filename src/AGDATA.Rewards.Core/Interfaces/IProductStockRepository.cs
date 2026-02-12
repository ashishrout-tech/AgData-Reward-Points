using Project.Domain.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IProductStockAsyncRepository
    {
        Task<ProductStock> AddAsync(ProductStock productStock, CancellationToken cancellationToken = default);
        Task<ProductStock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task UpdateStockAsync(Guid productId, int newStock, CancellationToken cancellationToken = default);
    }
}
