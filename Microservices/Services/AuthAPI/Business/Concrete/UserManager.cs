using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        IUserDal _userDal;
        IRedisService _redisService;

        public UserManager(IUserDal userDal, IRedisService redisService)
        {
            _userDal = userDal;
            _redisService = redisService;
        }

        public IDataResult<User> FindByEmail(string email)
        {
            User user = _userDal.Get(u => u.Email == email);

            if (user != null)
            {
                return new SuccessDataResult<User>(user, Messages.EmailFound);
            }
            else
            {
                return new SuccessDataResult<User>(Messages.EmailNotFound);
            }
        }

        public IDataResult<string> GenerateVerificationCode(string email)
        {
            Random random = new Random();
            int verificationCode = random.Next(100000, 999999);

            var result = _redisService.SaveVerificationCode(email, verificationCode.ToString());

            if (result.Success)
            {
                return new SuccessDataResult<string>(verificationCode.ToString(), "Verification code generated and saved to Redis.");
            }
            else
            {
                return new SuccessDataResult<string>(result.Message); // ErrorDataResult
            }
        }

        public IDataResult<User> GetOne(FilterOptions filterOptions, bool isEmail)
        {
            Expression<Func<User, bool>> filterExpression;

            if (isEmail)
            {
                filterExpression = u => u.Email == filterOptions.Email;
            }
            else
            {
                filterExpression = u => u.PhoneNumber == filterOptions.PhoneNumber;
            }

            var user = _userDal.GetByEmailOrPhoneNumber(filterExpression);

            if (user != null)
            {
                return new SuccessDataResult<User>(user, Messages.UserFound);
            }
            else
            {
                return new SuccessDataResult<User>(Messages.UserNotFound);
            }
        }

        public IDataResult<User> Add(User user)
        {
            _userDal.Add(user);
            return new SuccessDataResult<User>(user, "User created successfully");
        }
    }
}
