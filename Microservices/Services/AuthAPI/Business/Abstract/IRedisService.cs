using Core.Utilities.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IRedisService
    {
        IDataResult<string> GetAsync(string key);
        IDataResult<bool> SetAsync(string key, string value, int expiresInSeconds);
        IDataResult<string> SaveVerificationCode(string email, string code);
        IDataResult<bool> DeleteKey(string email);
    }
}
