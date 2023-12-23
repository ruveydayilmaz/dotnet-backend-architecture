using Business.Abstract;
using Core.Utilities.Results;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class RedisManager : IRedisService
    {
        private readonly IDistributedCache _cache;

        public RedisManager(IDistributedCache cache)
        {
            _cache = cache;
        }

        public IDataResult<string> GetAsync(string key)
        {
            var storedCodeBytes = _cache.GetAsync(key).Result;

            if (storedCodeBytes != null)
            {
                var storedCode = Encoding.UTF8.GetString(storedCodeBytes);

                return new SuccessDataResult<string>(storedCode, "stored code");
            }
            else
            {
                return new SuccessDataResult<string>("Key not found");
            }
        }

        public IDataResult<bool> SetAsync(string key, string value, int expiresInSeconds)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresInSeconds),
            };

            _cache.SetStringAsync(key, value, options);
            return new SuccessDataResult<bool>(true);
        }

        public IDataResult<string> SaveVerificationCode(string email, string code)
        {
            string key = $"verification_code:{email}";

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            };

            _cache.SetString(key, code, options);

            return new SuccessDataResult<string>(code, "Code saved to Redis.");
        }
    }
}
