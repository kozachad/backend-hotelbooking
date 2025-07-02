using HotelBookingSystem.Entities;
using HotelBookingSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace HotelBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly MongoContext _mongo;

        public CommentsController(MongoContext mongo)
        {
            _mongo = mongo;
        }

        // GET api/comments/{hotelId}
        [HttpGet("{hotelId}")]
        public IActionResult GetComments(string hotelId)
        {
             var filter = Builders<Comment>.Filter.Eq(x => x.HotelId, hotelId);
            var comments = _mongo.Comments.Find(filter).ToList();
            return Ok(comments);
        }

        // POST api/comments
        //[Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            comment.Id = null;
            comment.CommentId = Guid.NewGuid().ToString();
            comment.CreatedAt = DateTime.UtcNow;

            await _mongo.Comments.InsertOneAsync(comment);
            return Ok(comment);
        }

        // GET api/comments/statistics/{hotelId}
        //[Authorize(Roles = "Admin")]
        [HttpGet("statistics/{hotelId}")]
        public IActionResult GetCommentStatistics(Guid hotelId)
        {
            var filter = Builders<Comment>.Filter.Eq(x => x.HotelId, hotelId.ToString());
            var comments = _mongo.Comments.Find(filter).ToList();

            var stats = comments
                .GroupBy(c => c.Rating)
                .Select(g => new
                {
                    Rating = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return Ok(stats);
        }

    }
}
