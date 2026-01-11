using System.ComponentModel.DataAnnotations;

namespace Project.Application.DTOs.Product
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Product description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Product description must be between 10 and 1000 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Brand is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Brand must be between 2 and 100 characters")]
        public string Brand { get; set; } = null!;

        [Required(ErrorMessage = "Points price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Points price must be greater than 0")]
        public decimal PointsPrice { get; set; }

        [Required(ErrorMessage = "Initial stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Initial stock must be non-negative")]
        public int InitialStock { get; set; }
    }

    public class UpdateProductRequest
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string? Name { get; set; }

        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Product description must be between 10 and 1000 characters")]
        public string? Description { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Brand must be between 2 and 100 characters")]
        public string? Brand { get; set; }
    }

    public class UpdateProductPriceRequest
    {
        [Required(ErrorMessage = "Points price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Points price must be greater than 0")]
        public decimal PointsPrice { get; set; }
    }

    public class UpdateProductStockRequest
    {
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
        public int Quantity { get; set; }
    }

    public class DecreaseStockRequest
    {
        [Required(ErrorMessage = "Quantity to decrease is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
