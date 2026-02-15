namespace AGDATA.Rewards.Core.ProductAggregate;

public sealed class ProductStock
{
  public Guid ProductId { get; }
  public int AvailableStock { get; private set; }
  private ProductStock() { }

  public ProductStock(Guid productId, int initialStock)
  {
    ProductId = productId;
    AvailableStock = initialStock;
  }

  public void UpdateStock(int newStock)
  {
    if (newStock < 0)
      throw new ArgumentOutOfRangeException(nameof(newStock), "Stock cannot be negative.");
    else if(newStock > 10000)
      throw new ArgumentOutOfRangeException(nameof(newStock), "Stock cannot exceed 10,000.");
    AvailableStock = newStock;
  }
}
