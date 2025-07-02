namespace HotelBookingSystem.DTOs
{
    public class SendCommentDto
    {
        public string CommentId { get; set; }

        public string HotelId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
