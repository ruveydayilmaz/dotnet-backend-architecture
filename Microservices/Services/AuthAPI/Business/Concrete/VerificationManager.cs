using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class VerificationManager : IVerificationService
    {
        private readonly IRedisService _redisService;

        public VerificationManager(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public IDataResult<bool> CheckVerificationCode(string key, string code)
        {
            var storedCode = _redisService.GetAsync(key);

            if (!storedCode.Success)
            {
                return new SuccessDataResult<bool>("Error retrieving verification code from Redis");
            }

            var result = storedCode.Data == code;

            return new SuccessDataResult<bool>(result, result ? "Verification code is valid." : "Invalid verification code.");
        }

        public IDataResult<bool> SetVerified(string emailOrPhone)
        {
            const string value = "true";
            const int ONE_DAY = 24 * 60 * 60; // seconds

            _redisService.SetAsync(emailOrPhone, value, ONE_DAY);
            return new SuccessDataResult<bool>(true);
        }
    }
}
