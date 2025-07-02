using HotelBookingSystem.Entities;
using MongoDB.Driver;

namespace HotelBookingSystem.Helpers
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(IConfiguration configuration)
        {
            var settings = configuration.GetSection("MongoDbSettings");
            var client = new MongoClient(settings["ConnectionString"]);
            _database = client.GetDatabase(settings["DatabaseName"]);
        }

        public IMongoCollection<Comment> Comments
            => _database.GetCollection<Comment>("Comments");
    }
}
