using System;
using System.Collections.Generic;
using System.Text;
using AGDATA.Rewards.Core.Common;

namespace AGDATA.Rewards.Core.EventAggregate.ValueObjects;

public sealed class EventTitle : ValueObject
{
  public string Value { get; }

  public EventTitle(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("Event title cannot be null or empty.", nameof(value));
    if (value.Length > 200)
      throw new ArgumentException("Event title cannot exceed 200 characters.", nameof(value));
    Value = value;
  }
  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
