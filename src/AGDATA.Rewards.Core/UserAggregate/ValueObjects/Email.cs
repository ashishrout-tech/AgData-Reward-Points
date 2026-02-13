using AGDATA.Rewards.Core.Common;
using System.Text.RegularExpressions;

namespace AGDATA.Rewards.Core.UserAggregate.ValueObjects;

public sealed class Email : ValueObject
{
  public string Value { get; }

  public Email(string value)
  {
    if (string.IsNullOrWhiteSpace(value) || !IsValidEmail(value))
      throw new ArgumentException("Invalid email format.", nameof(value));
    
    Value = value;
  }
  
  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }

  private static bool IsValidEmail(string email)
  {
    if (email.Length > 254 || !email.EndsWith("@agdata.com", StringComparison.OrdinalIgnoreCase))
      return false;

    var emailPattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@agdata\.com$";
    return Regex.IsMatch(email, emailPattern);
  }
}
