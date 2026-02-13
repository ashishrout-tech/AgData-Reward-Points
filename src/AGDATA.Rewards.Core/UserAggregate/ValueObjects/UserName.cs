using System;
using System.Collections.Generic;
using System.Text;
using AGDATA.Rewards.Core.Common;

namespace AGDATA.Rewards.Core.UserAggregate.ValueObjects;

public sealed class UserName : ValueObject
{
  public string Value { get; }

  public UserName(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("User name cannot be null or empty.", nameof(value));
    if (value.Length > 100)
      throw new ArgumentException("User name cannot exceed 100 characters.", nameof(value));
    Value = value;
  }
  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString() => Value;
}
