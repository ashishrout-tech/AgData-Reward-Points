using Project.Application.DTOs.Product;

namespace Project.Application.Services
{
    public interface IProductService
    {
        // CRUD operations
        Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
        Task<ProductDto> GetProductAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

        // Status operations
        Task<ProductDto> ActivateProductAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProductDto> DeactivateProductAsync(Guid id, CancellationToken cancellationToken = default);

        // Price operations
        Task<ProductPriceDto> GetProductPriceAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductPriceDto> UpdateProductPriceAsync(Guid productId, decimal pointsPrice, CancellationToken cancellationToken = default);

        // Stock operations
        Task<ProductStockDto> GetProductStockAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductStockDto> UpdateProductStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
        Task<ProductStockDto> DecreaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);

        // Search operations
        Task<List<ProductDto>> SearchProductsAsync(string? name, string? brand, CancellationToken cancellationToken = default);
    }
}
