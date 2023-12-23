using Core.Utilities.Results;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IUserService
    {
        IDataResult<User> FindByEmail(string email);
        IDataResult<string> GenerateVerificationCode(string email);
        IDataResult<User> GetOne(FilterOptions filterOptions, bool isEmail);
        IDataResult<User> Add(User user);
    }
}

public class FilterOptions
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsEmail { get; set; }
}