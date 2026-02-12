using System;

namespace Project.Application.DTOs.User
{
    public class UserAccountDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int Points { get; set; }
    }
}
