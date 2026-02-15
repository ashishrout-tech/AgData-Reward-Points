namespace AGDATA.Rewards.Core.ProductAggregate;

public sealed class ProductPoint
{
  public Guid ProductId { get; }
  public int CurrentPoints { get; private set; }
  private ProductPoint() { }

  internal ProductPoint(Guid productId, int initialPoints)
  {
    ProductId = productId;
    CurrentPoints = initialPoints;
  }

  internal void UpdatePoint(int newPoints)
  {
    if (newPoints < 0)
      throw new ArgumentOutOfRangeException(nameof(newPoints), "Points cannot be negative.");
    else if (newPoints > 1000_000)
      throw new ArgumentOutOfRangeException(nameof(newPoints), "Points cannot exceed 1,000,000.");

    CurrentPoints = newPoints;
  }
}
