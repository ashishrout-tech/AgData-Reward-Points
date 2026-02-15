using AGDATA.Rewards.Core.Common;
using AGDATA.Rewards.Core.ProductAggregate.ValueObjects;
using Project.Domain.Entities;

namespace AGDATA.Rewards.Core.ProductAggregate;

public class Product : IAggregateRoot
{
  public Guid Id { get; }
  public ProductName Name { get; private set; }
  public string Description { get; private set; }
  public ProductBrand Brand { get; private set; }
  public bool IsActive { get; private set; }
  public Guid? PhotoId { get; private set; }
  public ProductPoint ProductPoint { get; private set; }
  public ProductStock ProductStock { get; private set; }
  public Photo? Photo { get; private set; }
  private Product() { }
  public Product(ProductName name, string description, ProductBrand brand)
  {
    if (string.IsNullOrWhiteSpace(description))
      throw new ArgumentException("Product description cannot be null or empty.", nameof(description));

    Id = Guid.NewGuid();
    Name = name;
    Description = description;
    Brand = brand;
    IsActive = true;
    ProductPoint = new ProductPoint(Id, 0);
    ProductStock = new ProductStock(Id, 0);
  }
  public void SetPhoto(Guid photoId)
  {
    ThrowIfInactive();
    if (photoId == Guid.Empty)
      throw new ArgumentException("PhotoId cannot be empty.", nameof(photoId));
    PhotoId = photoId;
  }

  public void UpdateDetails(ProductName? name, string? description, ProductBrand? brand)
  {
    ThrowIfInactive();
    Name = name ?? Name;
    Description = description ?? Description;
    Brand = brand ?? Brand;
  }

  public void UpdatePoint(int newPoints)
  {
    ThrowIfInactive();
    ProductPoint.UpdatePoint(newPoints);
  }

  public void UpdateStock(int newStock)
  {
    ThrowIfInactive();
    ProductStock.UpdateStock(newStock);
  }

  public void Deactivate() => IsActive = false;

  public void Activate() => IsActive = true;

  private void ThrowIfInactive()
  {
    if (!IsActive)
      throw new InvalidOperationException("Operation cannot be performed on an inactive product.");
  }
}
