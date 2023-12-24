using Business.Abstract;
using Core.Utilities.Results;
using StackExchange.Redis;

namespace Business.Concrete
{
    public class RedisManager : IRedisService
    {
        private readonly IConnectionMultiplexer _redisConnection;

        public RedisManager(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
        }

        public IDataResult<string> GetAsync(string key)
        {
            var redisDatabase = _redisConnection.GetDatabase();
            var storedCodeBytes = redisDatabase.StringGet(key);

            if (!storedCodeBytes.IsNullOrEmpty)
            {
                var storedCode = storedCodeBytes.ToString();

                return new SuccessDataResult<string>(storedCode, "stored code");
            }
            else
            {
                return new SuccessDataResult<string>("Key not found");
            }
        }

        public IDataResult<bool> SetAsync(string key, string value, int expiresInSeconds)
        {
            var redisDatabase = _redisConnection.GetDatabase();
            var success = redisDatabase.StringSet(key, value, TimeSpan.FromSeconds(expiresInSeconds));

            return new SuccessDataResult<bool>(success, "Data set in Redis.");
        }

        public IDataResult<bool> DeleteKey(string key)
        {
            var redisDatabase = _redisConnection.GetDatabase();
            var result = redisDatabase.KeyDelete(key);

            if (result)
            {
                return new SuccessDataResult<bool>(true, $"Key '{key}' deleted successfully");
            }
            else
            {
                return new SuccessDataResult<bool>(false, $"Key '{key}' not found in Redis"); // ErrorDataResult
            }
        }

        public IDataResult<string> SaveVerificationCode(string email, string code)
        {
            string key = $"verification_code:{email}";

            var redisDatabase = _redisConnection.GetDatabase();
            var success = redisDatabase.StringSet(key, code, TimeSpan.FromHours(1));

            return new SuccessDataResult<string>(success ? code : null, success ? "Code saved to Redis." : "Failed to save code to Redis.");
        }
    }
}
