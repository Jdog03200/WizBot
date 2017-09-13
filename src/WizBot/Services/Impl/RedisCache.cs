using StackExchange.Redis;
using System.Threading.Tasks;

namespace WizBot.Services.Impl
{
    public class RedisCache : IDataCache
    {
        public ConnectionMultiplexer Redis { get; }
        private readonly IDatabase _db;

        public RedisCache()
        {
            Redis = ConnectionMultiplexer.Connect("localhost");
            Redis.PreserveAsyncOrder = false;
            _db = Redis.GetDatabase();
        }

        public async Task<(bool Success, byte[] Data)> TryGetImageDataAsync(string key)
        {
            byte[] x = await _db.StringGetAsync("image_" + key);
            return (x != null, x);
        }

        public Task SetImageDataAsync(string key, byte[] data)
        {
            return _db.StringSetAsync("image_" + key, data);
        }
    }
}