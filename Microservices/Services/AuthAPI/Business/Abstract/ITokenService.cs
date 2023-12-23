using Core.Utilities.Results;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ITokenService
    {
        IDataResult<string> CreateAccessToken(string id);
        IDataResult<string> CreateRefreshToken(string id);
    }
}
