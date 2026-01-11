using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Product;
using Project.Application.Services;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active products with prices and stock
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all active products</returns>
        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving all products");
                var products = await _productService.GetAllProductsAsync(cancellationToken);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving products: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving products" });
            }
        }

        /// <summary>
        /// Get single product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Product details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving product: {ProductId}", id);
                var product = await _productService.GetProductAsync(id, cancellationToken);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving product: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving product" });
            }
        }

        /// <summary>
        /// Create new product (Admin only)
        /// </summary>
        /// <param name="request">Product creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductDto>> CreateProduct(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating product: {ProductName}", request.Name);
                var product = await _productService.CreateProductAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating product: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating product" });
            }
        }

        /// <summary>
        /// Update product details (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="request">Update details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(
            Guid id,
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating product: {ProductId}", id);
                var product = await _productService.UpdateProductAsync(id, request, cancellationToken);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating product" });
            }
        }

        /// <summary>
        /// Activate deactivated product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Activated product</returns>
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductDto>> ActivateProduct(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Activating product: {ProductId}", id);
                var product = await _productService.ActivateProductAsync(id, cancellationToken);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error activating product: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while activating product" });
            }
        }

        /// <summary>
        /// Deactivate product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deactivated product</returns>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductDto>> DeactivateProduct(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deactivating product: {ProductId}", id);
                var product = await _productService.DeactivateProductAsync(id, cancellationToken);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deactivating product: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while deactivating product" });
            }
        }

        /// <summary>
        /// Get product points price
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Product price information</returns>
        [HttpGet("{id}/price")]
        public async Task<ActionResult<ProductPriceDto>> GetProductPrice(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving product price: {ProductId}", id);
                var price = await _productService.GetProductPriceAsync(id, cancellationToken);
                return Ok(price);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving product price: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving product price" });
            }
        }

        /// <summary>
        /// Update product points price (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="request">New price details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated price information</returns>
        [HttpPut("{id}/price")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductPriceDto>> UpdateProductPrice(
            Guid id,
            [FromBody] UpdateProductPriceRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating product price: {ProductId}", id);
                var price = await _productService.UpdateProductPriceAsync(id, request.PointsPrice, cancellationToken);
                return Ok(price);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning("Invalid price value: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product price: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating product price" });
            }
        }

        /// <summary>
        /// Get product stock status (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stock information</returns>
        [HttpGet("{id}/stock")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductStockDto>> GetProductStock(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving product stock: {ProductId}", id);
                var stock = await _productService.GetProductStockAsync(id, cancellationToken);
                return Ok(stock);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving product stock: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving product stock" });
            }
        }

        /// <summary>
        /// Update product stock quantity (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="request">New stock details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated stock information</returns>
        [HttpPut("{id}/stock")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductStockDto>> UpdateProductStock(
            Guid id,
            [FromBody] UpdateProductStockRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating product stock: {ProductId}", id);
                var stock = await _productService.UpdateProductStockAsync(id, request.Quantity, cancellationToken);
                return Ok(stock);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning("Invalid stock value: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product stock: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating product stock" });
            }
        }

        /// <summary>
        /// Decrease product stock when redeemed (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="request">Quantity to decrease</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated stock information</returns>
        [HttpPost("{id}/stock/decrease")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ProductStockDto>> DecreaseProductStock(
            Guid id,
            [FromBody] DecreaseStockRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Decreasing product stock: {ProductId}", id);
                var stock = await _productService.DecreaseStockAsync(id, request.Quantity, cancellationToken);
                return Ok(stock);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning("Invalid quantity: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error decreasing product stock: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while decreasing product stock" });
            }
        }

        /// <summary>
        /// Search products by name and/or brand
        /// </summary>
        /// <param name="name">Product name (optional)</param>
        /// <param name="brand">Brand name (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching products</returns>
        [HttpGet("search/query")]
        public async Task<ActionResult<List<ProductDto>>> SearchProducts(
            [FromQuery] string? name,
            [FromQuery] string? brand,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Searching products: Name={Name}, Brand={Brand}", name, brand);
                var products = await _productService.SearchProductsAsync(name, brand, cancellationToken);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error searching products: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while searching products" });
            }
        }
    }
}
