using HotelBookingSystem.Data;
using HotelBookingSystem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public RoomController(HotelDbContext context)
        {
            _context = context;
        }

        // GET api/room

        [HttpGet]
        public IActionResult GetRooms()
        {
            var rooms = _context.Rooms.ToList();
            return Ok(rooms);
        }

        [HttpGet("search")]
        public IActionResult GetRoomsByHotelId([FromQuery] string hotelId)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(hotelId) && Guid.TryParse(hotelId, out Guid hotelGuid))
            {
                query = query.Where(r => r.HotelId == hotelGuid);
            }

            var rooms = query
                .Select(r => new
                {
                    r.RoomId,
                    r.RoomType,
                    r.TotalCount,
                    r.AvailableCount,
                    r.PricePerNight,
                    r.StartDate,
                    r.EndDate
                })
                .ToList();

            return Ok(rooms);
        }



        // GET api/room/{id}
        [HttpGet("{id}")]
        public IActionResult GetRoomById(Guid id)
        {
            var room = _context.Rooms.FirstOrDefault(x => x.RoomId == id);
            if (room == null)
                return NotFound();

            return Ok(room);
        }

        // POST api/room
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateRoom([FromBody] Room room)
        {
            room.RoomId = Guid.NewGuid();

            _context.Rooms.Add(room);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetRoomById), new { id = room.RoomId }, room);
        }

        // PUT api/room/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateRoom(Guid id, [FromBody] Room updatedRoom)
        {
            var room = _context.Rooms.FirstOrDefault(x => x.RoomId == id);
            if (room == null)
                return NotFound();

            room.HotelId = updatedRoom.HotelId;
            room.RoomType = updatedRoom.RoomType;
            room.TotalCount = updatedRoom.TotalCount;
            room.AvailableCount = updatedRoom.AvailableCount;
            room.PricePerNight = updatedRoom.PricePerNight;
            room.StartDate = updatedRoom.StartDate;
            room.EndDate = updatedRoom.EndDate;

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE api/room/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteRoom(Guid id)
        {
            var room = _context.Rooms.FirstOrDefault(x => x.RoomId == id);
            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
