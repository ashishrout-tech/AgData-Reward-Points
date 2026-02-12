using System;
using System.Collections.Generic;
using System.Text;

namespace AGDATA.Rewards.Core.Common;


public abstract class ValueObject
{
  protected abstract IEnumerable<object> GetEqualityComponents();

  public override bool Equals(object? obj)
  {
    if (obj is null || obj.GetType() != GetType())
      return false;

    var other = (ValueObject)obj;

    return GetEqualityComponents()
        .SequenceEqual(other.GetEqualityComponents());
  }

  public override int GetHashCode()
      => GetEqualityComponents()
          .Aggregate(1, (hash, component) =>
              HashCode.Combine(hash, component?.GetHashCode() ?? 0));
}
