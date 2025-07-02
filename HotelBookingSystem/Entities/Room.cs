namespace HotelBookingSystem.Entities
{
    public class Room
    {
        public Guid RoomId { get; set; }
        public Guid HotelId { get; set; }
        public string RoomType { get; set; }
        public int TotalCount {  get; set; }
        public int AvailableCount { get; set; }
        public decimal PricePerNight { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
