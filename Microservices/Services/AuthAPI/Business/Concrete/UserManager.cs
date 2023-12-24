using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System.Linq.Expressions;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        IUserDal _userDal;

        public UserManager(IUserDal userDal)
        {
            _userDal = userDal;
        }

        public IDataResult<User> GetByEmail(string email)
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

        public IDataResult<User> GetById(string id)
        {
            User user = _userDal.Get(u => u.Id == id);

            if (user != null)
            {
                return new SuccessDataResult<User>(user, "user found");
            }
            else
            {
                return new SuccessDataResult<User>("user not found");
            }
        }

        public IDataResult<User> GetOneByEmailOrPhoneNumber(FilterOptions filterOptions, bool isEmail)
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

        public IDataResult<User> UpdatePassword(User user, string newPassword)
        {
            var updatedUser = _userDal.UpdatePassword(user, newPassword);

            return new SuccessDataResult<User>(updatedUser, "User updated successfully");
        }
    }
}
