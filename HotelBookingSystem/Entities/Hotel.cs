﻿namespace HotelBookingSystem.Entities
{
    public class Hotel
    {
        public Guid HotelId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
