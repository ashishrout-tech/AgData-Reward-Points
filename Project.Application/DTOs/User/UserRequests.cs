using Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Application.DTOs.User
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Employee ID is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Employee ID must be between 2 and 50 characters")]
        public string EmployeeId { get; set; } = null!;

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, digit, and special character")]
        public string Password { get; set; } = null!;
    }

    public class UpdateUserRequest
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Employee ID must be between 2 and 50 characters")]
        public string? EmployeeId { get; set; }

        public UserRole? Role { get; set; }

        public bool? IsActive { get; set; }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    public class CreateUserAccountRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }
    }

    public class UpdatePointsRequest
    {
        [Required(ErrorMessage = "Points value is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Points must be non-negative")]
        public int Points { get; set; }
    }
}
