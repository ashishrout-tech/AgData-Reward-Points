using AGDATA.Rewards.Core.Common;
using AGDATA.Rewards.Core.UserAggregate.ValueObjects;
using Project.Domain.Entities;

namespace AGDATA.Rewards.Core.UserAggregate;

public sealed class User : IAggregateRoot
{
  public Guid Id { get; }
  public UserName Name { get; private set; }
  public Email Email { get; private set; }
  public string EmployeeId { get; private set; }
  public Guid? PhotoId { get; private set; }
  public UserRole Role { get; private set; }
  public bool IsActive { get; private set; }
  public string PasswordHash { get; private set; }

  public UserAccount UserAccount { get; private set; } = null!;
  public Photo? Photo { get; private set; }

  private User() { }

  public User(UserName name, Email email, string employeeId, UserRole role, string password)
  {
    if (string.IsNullOrWhiteSpace(employeeId))
      throw new ArgumentException("EmployeeId cannot be null or empty.", nameof(employeeId));
    if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
      throw new ArgumentException("Password must be at least 8 characters.", nameof(password));
    if (!Enum.IsDefined(role))
      throw new ArgumentException("Invalid user role.", nameof(role));

    Id = Guid.NewGuid();
    Name = name;
    Email = email;
    EmployeeId = employeeId;
    Role = role;
    IsActive = true;
    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
    UserAccount = new UserAccount(Id);
  }

  public bool VerifyPassword(string password)
  {
    return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
  }

  public bool ChangePassword(string? currentPassword, string newPassword)
  {
    if (currentPassword != null && !VerifyPassword(currentPassword))
      throw new UnauthorizedAccessException("Current password is incorrect.");
    if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
      throw new ArgumentException("New password must be at least 8 characters.", nameof(newPassword));
    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
    return true;
  }

  public void UpdateUserRole(UserRole role)
  {
    Role = role;
  }

  public void DeactivateUser()
  {
    IsActive = false;
  }

  public void ActivateUser()
  {
    IsActive = true;
  }

  public bool IsUserActive()
  {
    return IsActive;
  }

  public void SetPhoto(Guid photoId)
  {
    if (photoId == Guid.Empty)
      throw new ArgumentException("PhotoId cannot be empty.", nameof(photoId));
    PhotoId = photoId;
  }
}
