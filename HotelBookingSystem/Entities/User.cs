﻿namespace HotelBookingSystem.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Admin" veya "User"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
