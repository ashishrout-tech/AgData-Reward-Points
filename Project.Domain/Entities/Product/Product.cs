using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities.Product
{
    public class Product
    {
        public Guid Id { get; }
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string Brand { get; private set; } = null!;
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public bool IsActive { get; private set; }
        public ProductPrice ProductPrice { get; private set; } = null!;
        public ProductStock ProductStock { get; private set; } = null!;
        private Product() {}
        public Product(string name, string description, string brand)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Product description cannot be null or empty.", nameof(description));
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Product brand cannot be null or empty.", nameof(brand));

            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Brand = brand;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void SetPriceAndStock(ProductPrice price, ProductStock stock)
        {
            if (price == null)
                throw new ArgumentNullException(nameof(price));
            if (stock == null)
                throw new ArgumentNullException(nameof(stock));
            if (price.ProductId != this.Id)
                throw new ArgumentException("ProductPrice must belong to this product.");
            if (stock.ProductId != this.Id)
                throw new ArgumentException("ProductStock must belong to this product.");

            ProductPrice = price;
            ProductStock = stock;
        }

        public void UpdateDetails(string name, string description, string brand)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Product description cannot be null or empty.", nameof(description));
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Product brand cannot be null or empty.", nameof(brand));

            Name = name;
            Description = description;
            Brand = brand;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException("Product is already deactivated.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive)
                throw new InvalidOperationException("Product is already active.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
