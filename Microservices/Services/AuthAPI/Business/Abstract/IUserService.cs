using Core.Utilities.Results;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IUserService
    {
        IDataResult<User> GetByEmail(string email);
        IDataResult<User> GetById(string id);
        IDataResult<User> GetOneByEmailOrPhoneNumber(FilterOptions filterOptions, bool isEmail);
        IDataResult<User> Add(User user);
        IDataResult<User> UpdatePassword(User user, string newPassword);
    }
}

public class FilterOptions
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsEmail { get; set; }
}