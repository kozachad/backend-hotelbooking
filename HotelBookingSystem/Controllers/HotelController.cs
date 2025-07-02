using HotelBookingSystem.Data;
using HotelBookingSystem.Entities;
using HotelBookingSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly HotelCacheService _cacheService;
        private readonly HotelDbContext _context;

        public HotelController(HotelDbContext context, HotelCacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        // GET api/hotel
        [HttpGet]
        public IActionResult GetHotels()
        {
            var hotels = _context.Hotels.ToList();
            return Ok(hotels);
        }


        [HttpGet("search")]
        public IActionResult SearchHotels(
            [FromQuery] string? city,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? guestCount)
        {
            // Dummy olarak tüm otelleri getiriyoruz (veya city varsa filtreliyoruz)
            var hotels = _context.Hotels
                .Where(h => string.IsNullOrEmpty(city) || h.City.ToLower().Contains(city.ToLower()))
                .ToList();

            var results = new List<object>();

            foreach (var hotel in hotels)
            {
                var availableRooms = _context.Rooms
                    .Where(r => r.HotelId == hotel.HotelId)
                    .ToList();

                if (availableRooms.Any())
                {
                    results.Add(new
                    {
                        hotel.HotelId,
                        hotel.Name,
                        hotel.City,
                        hotel.Country,
                        hotel.Address,
                        hotel.Description,
                        StartDate = startDate?.ToString("yyyy-MM-dd"),
                        EndDate = endDate?.ToString("yyyy-MM-dd"),
                        GuestCount = guestCount ?? 0,
                        Rooms = availableRooms.Select(r => new
                        {
                            r.RoomId,
                            r.RoomType,
                            r.AvailableCount,
                            r.PricePerNight,
                            RoomStartDate = r.StartDate.ToString("yyyy-MM-dd"),
                            RoomEndDate = r.EndDate.ToString("yyyy-MM-dd")
                        })
                    });
                }
            }

            return Ok(results);
        }
        // GET api/hotel/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(string id)
        {
            var cached = await _cacheService.GetHotelAsync(id);
            if (!string.IsNullOrEmpty(cached))
            {
                return Ok(cached);
            }

            var hotel = _context.Hotels.FirstOrDefault(x => x.HotelId.ToString() == id);
            if (hotel == null)
                return NotFound();

            var hotelJson = System.Text.Json.JsonSerializer.Serialize(hotel);
            return Ok(hotelJson);
        }

        // POST api/hotel
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateHotel([FromBody] Hotel hotel)
        {
            hotel.HotelId = Guid.NewGuid();
            hotel.CreatedAt = DateTime.UtcNow;

            _context.Hotels.Add(hotel);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetHotelById), new { id = hotel.HotelId }, hotel);
        }

        // PUT api/hotel/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateHotel(Guid id, [FromBody] Hotel updatedHotel)
        {
            var hotel = _context.Hotels.FirstOrDefault(x => x.HotelId == id);
            if (hotel == null)
                return NotFound();

            hotel.Name = updatedHotel.Name;
            hotel.City = updatedHotel.City;
            hotel.Country = updatedHotel.Country;
            hotel.Address = updatedHotel.Address;
            hotel.Description = updatedHotel.Description;

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE api/hotel/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteHotel(Guid id)
        {
            var hotel = _context.Hotels.FirstOrDefault(x => x.HotelId == id);
            if (hotel == null)
                return NotFound();

            _context.Hotels.Remove(hotel);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
