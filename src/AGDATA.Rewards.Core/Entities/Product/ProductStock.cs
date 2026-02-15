//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Project.Domain.Entities.Product
//{
//    public class ProductStock
//    {
//        public Guid ProductId { get; }
//        public int AvailableStock { get; private set; }
//        public Product Product { get; private set; } = null!;
//        private ProductStock() { }

//        public ProductStock(Guid productId, int initialStock)
//        {
//            if (productId == Guid.Empty)
//                throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
//            if (initialStock < 0)
//                throw new ArgumentOutOfRangeException(nameof(initialStock), "Initial stock cannot be negative.");

//            ProductId = productId;
//            AvailableStock = initialStock;
//        }

//        public void UpdateStock(int newStock)
//        {
//            if (newStock < 0)
//                throw new ArgumentOutOfRangeException(nameof(newStock), "Stock cannot be negative.");

//            AvailableStock = newStock;
//        }
//    }
//}
