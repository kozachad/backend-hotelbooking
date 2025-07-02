using StackExchange.Redis;

namespace HotelBookingSystem.Helpers
{
    public class HotelCacheService
    {
        private readonly IDatabase _redisDb;

        public HotelCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _redisDb = connectionMultiplexer.GetDatabase();
        }

        public async Task CacheHotelAsync(string hotelId, string hotelJson)
        {
            await _redisDb.StringSetAsync($"hotel:{hotelId}", hotelJson, TimeSpan.FromMinutes(30));
        }

        public async Task<string?> GetHotelAsync(string hotelId)
        {
            return await _redisDb.StringGetAsync($"hotel:{hotelId}");
        }
    }
}
