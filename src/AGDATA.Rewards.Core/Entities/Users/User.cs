using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Domain.Enums;
using BCrypt.Net;
using Project.Domain.Entities.Auth;

namespace Project.Domain.Entities.Users
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string EmployeeId { get; private set; } = null!;
        public Guid? PhotoId { get; private set; }
		public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }
        public string PasswordHash { get; private set; } = null!;

        public UserAccount UserAccount { get; private set; } = null!;
        public Photo? Photo { get; private set; }

		private User() { }

        public User(string name, string email, string employeeId, UserRole role, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("EmployeeId cannot be null or empty.", nameof(employeeId));
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.", nameof(password));
            if (!Enum.IsDefined(typeof(UserRole), role))
                throw new ArgumentException("Invalid user role.", nameof(role));

            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            EmployeeId = employeeId;
            Role = role;
            IsActive = true;
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
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

		public void CreateUserAccount()
        {
            if (UserAccount != null)
                throw new InvalidOperationException("User account already exists for this user.");

            UserAccount = new UserAccount(Id);
        }
    }
}
