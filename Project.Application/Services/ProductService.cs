using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Application.DTOs.Product;
using Project.Domain.Entities.Product;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductAsyncRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductAsyncRepository productRepository,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating product: {ProductName}", request.Name);

                var product = new Product(request.Name, request.Description, request.Brand);

                var productPrice = new ProductPrice(product.Id, request.PointsPrice);

                var productStock = new ProductStock(product.Id, request.InitialStock);

                product.SetPriceAndStock(productPrice, productStock);

                var createdProduct = await _productRepository.AddAsync(product, cancellationToken);

                _logger.LogInformation("Product created successfully: {ProductId}", createdProduct.Id);

                return _mapper.Map<ProductDto>(createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating product: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductDto> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID '{id}' not found");
            }

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all products");

            var products = await _productRepository.GetAllAsync(cancellationToken);
            var activeProducts = products.Where(p => p.IsActive).ToList();
            return _mapper.Map<List<ProductDto>>(activeProducts);
        }

        public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating product: {ProductId}", id);

                var product = await _productRepository.GetByIdAsync(id, cancellationToken);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID '{id}' not found");
                }

                if (!string.IsNullOrEmpty(request.Name) || !string.IsNullOrEmpty(request.Description) || !string.IsNullOrEmpty(request.Brand))
                {
                    var name = request.Name ?? product.Name;
                    var description = request.Description ?? product.Description;
                    var brand = request.Brand ?? product.Brand;

                    product.UpdateDetails(name, description, brand);
                }

                await _productRepository.UpdateAsync(product, cancellationToken);

                _logger.LogInformation("Product updated successfully: {ProductId}", id);

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductDto> ActivateProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Activating product: {ProductId}", id);

                var product = await _productRepository.GetByIdAsync(id, cancellationToken);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID '{id}' not found");
                }

                product.Activate();
                await _productRepository.UpdateAsync(product, cancellationToken);

                _logger.LogInformation("Product activated successfully: {ProductId}", id);

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error activating product: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductDto> DeactivateProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deactivating product: {ProductId}", id);

                var product = await _productRepository.GetByIdAsync(id, cancellationToken);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID '{id}' not found");
                }

                product.Deactivate();
                await _productRepository.UpdateAsync(product, cancellationToken);

                _logger.LogInformation("Product deactivated successfully: {ProductId}", id);

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deactivating product: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductPriceDto> GetProductPriceAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product price: {ProductId}", productId);

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID '{productId}' not found");
            }

            return _mapper.Map<ProductPriceDto>(product.ProductPrice);
        }

        public async Task<ProductPriceDto> UpdateProductPriceAsync(Guid productId, decimal pointsPrice, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating product price: {ProductId}", productId);

                var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID '{productId}' not found");
                }

                product.ProductPrice.UpdatePoints(pointsPrice);
                await _productRepository.UpdateAsync(product, cancellationToken);

                _logger.LogInformation("Product price updated successfully: {ProductId}", productId);

                return _mapper.Map<ProductPriceDto>(product.ProductPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product price: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductStockDto> GetProductStockAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product stock: {ProductId}", productId);

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID '{productId}' not found");
            }

            return _mapper.Map<ProductStockDto>(product.ProductStock);
        }

        public async Task<ProductStockDto> UpdateProductStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating product stock: {ProductId}", productId);

                var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID '{productId}' not found");
                }

                product.ProductStock.UpdateStock(quantity);
                await _productRepository.UpdateAsync(product, cancellationToken);

                _logger.LogInformation("Product stock updated successfully: {ProductId}", productId);

                return _mapper.Map<ProductStockDto>(product.ProductStock);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product stock: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductStockDto> DecreaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Decreasing product stock: {ProductId}", productId);

                var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID '{productId}' not found");
                }

                var currentStock = product.ProductStock.AvailableStock;
                if (currentStock < quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock. Available: {currentStock}, Requested: {quantity}");
                }

                product.ProductStock.UpdateStock(currentStock - quantity);
                await _productRepository.UpdateAsync(product, cancellationToken);

                _logger.LogInformation("Product stock decreased successfully: {ProductId}", productId);

                return _mapper.Map<ProductStockDto>(product.ProductStock);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error decreasing product stock: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<ProductDto>> SearchProductsAsync(string? name, string? brand, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching products: Name={Name}, Brand={Brand}", name, brand);

            var filteredProducts = await _productRepository.SearchAsync(name, brand, cancellationToken);

            _logger.LogInformation("Search found {ProductCount} products", filteredProducts.Count);

            return _mapper.Map<List<ProductDto>>(filteredProducts);
        }
    }
}
