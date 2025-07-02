using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HotelBookingSystem.Entities
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }
        public string? CommentId { get; set; }
        public string HotelId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
