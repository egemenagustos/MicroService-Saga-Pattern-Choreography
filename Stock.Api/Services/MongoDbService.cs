using MongoDB.Driver;

namespace Stock.Api.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase mongoDatabase;

        public MongoDbService(IConfiguration configuration)
        {
            MongoClient client = new(configuration["MongoDb"]);
            mongoDatabase = client.GetDatabase("StockDb");
        }

        public IMongoCollection<T> GetCollection<T>()
            => mongoDatabase.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}
