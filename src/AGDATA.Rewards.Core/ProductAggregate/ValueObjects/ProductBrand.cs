using System;
using System.Collections.Generic;
using System.Text;
using AGDATA.Rewards.Core.Common;

namespace AGDATA.Rewards.Core.ProductAggregate.ValueObjects;

public sealed class ProductBrand : ValueObject
{
  public string Value { get; }
  public ProductBrand(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("Product brand cannot be null or empty.", nameof(value));
    if (value.Length > 50)
      throw new ArgumentException("Product brand cannot exceed 50 characters.", nameof(value));
    Value = value;
  }
  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
