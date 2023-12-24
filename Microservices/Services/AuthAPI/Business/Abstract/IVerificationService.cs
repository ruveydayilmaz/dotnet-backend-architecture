using Core.Utilities.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IVerificationService
    {
        IDataResult<bool> CheckVerificationCode(string key, string code);
        IDataResult<bool> SetVerified(string key);
    }
}
