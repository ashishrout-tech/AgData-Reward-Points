using Project.Domain.Enums;
using System;

namespace Project.Application.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public Guid? PhotoId { get; set; }
		public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string EmployeeId { get; set; } = null!;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public UserAccountDto UserAccount { get; set; } = null!;
    }
}
