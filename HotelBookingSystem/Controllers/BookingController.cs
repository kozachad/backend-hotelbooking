using Azure.Messaging.ServiceBus;
using BookingService.Entities;
using HotelBookingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BookingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _queueName;
        public BookingController(HotelDbContext context, IConfiguration configuration)
        {
            _context = context;
            var connectionString = configuration.GetSection("ServiceBus")["ConnectionString"];
            _queueName = configuration.GetSection("ServiceBus")["QueueName"];
            _serviceBusClient = new ServiceBusClient(connectionString);
        }

        // GET api/booking
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetBookings()
        {
            var bookings = _context.Bookings.ToList();
            return Ok(bookings);
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetBookingsByUser(string userId)
        {
            var bookings = _context.Bookings
                .Where(b => b.UserId.ToString() == userId)
                .Select(b => new
                {
                    b.BookingId,
                    b.HotelId,
                    b.RoomId,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.GuestCount,
                    b.TotalPrice,
                    b.CreatedAt
                })
                .ToList();

            return Ok(bookings);
        }


        // GET api/booking/{id}
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetBookingById(Guid id)
        {
            var booking = _context.Bookings.FirstOrDefault(x => x.BookingId == id);
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        // POST api/booking
        //[Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            booking.BookingId = Guid.NewGuid();
            booking.CreatedAt = DateTime.UtcNow;

            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == booking.RoomId);
            booking.HotelId = room.HotelId;
            booking.TotalPrice = room.PricePerNight;

            if (room == null)
                return NotFound("Room not found.");

            if (room.AvailableCount <= 0)
                return BadRequest("No available rooms left.");

            room.AvailableCount -= 1;

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            var sender = _serviceBusClient.CreateSender(_queueName);

            var messageBody = System.Text.Json.JsonSerializer.Serialize(booking);
            var message = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(message);

            Console.WriteLine("Service Bus message sent!");

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.BookingId }, booking);
        }

        // PUT api/booking/{id}
        [Authorize(Roles = "User")]
        [HttpPut("{id}")]
        public IActionResult UpdateBooking(Guid id, [FromBody] Booking updatedBooking)
        {
            var booking = _context.Bookings.FirstOrDefault(x => x.BookingId == id);
            if (booking == null)
                return NotFound();

            booking.CheckInDate = updatedBooking.CheckInDate;
            booking.CheckOutDate = updatedBooking.CheckOutDate;
            booking.GuestCount = updatedBooking.GuestCount;
            booking.TotalPrice = updatedBooking.TotalPrice;

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE api/booking/{id}
        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(Guid id)
        {
            var booking = _context.Bookings.FirstOrDefault(x => x.BookingId == id);
            if (booking == null)
                return NotFound();

            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
