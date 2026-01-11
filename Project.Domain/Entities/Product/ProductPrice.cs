using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities.Product
{
    public class ProductPrice
    {
        public Guid ProductId { get; }
        public decimal CurrentPoints { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public Product Product { get; private set; } = null!;
        private ProductPrice() {}

        public ProductPrice(Guid productId, decimal initialPoints)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
            if (initialPoints < 0)
                throw new ArgumentOutOfRangeException(nameof(initialPoints), "Initial points cannot be negative.");

            ProductId = productId;
            CurrentPoints = initialPoints;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePoints(decimal newPoints)
        {
            if (newPoints < 0)
                throw new ArgumentOutOfRangeException(nameof(newPoints), "Points cannot be negative.");

            CurrentPoints = newPoints;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
