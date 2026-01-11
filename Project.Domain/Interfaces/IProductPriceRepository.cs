using Project.Domain.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IProductPriceAsyncRepository
    {
        Task<ProductPrice> AddAsync(ProductPrice productPrice, CancellationToken cancellationToken = default);
        Task<ProductPrice?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task UpdatePointsAsync(Guid productId, decimal newPoints, CancellationToken cancellationToken = default);
    }
}
