using Business.Abstract;
using Core.Utilities.Results;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class TokenManager : ITokenService
    {
        IRedisService _redisService;

        public TokenManager(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public IDataResult<string> CreateAccessToken(string id)
        {
            var secret = "eb86155ce6eec9a0d24e23265fc75682cd0e4977a332246d3b000a5671a96e1f";
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new SuccessDataResult<string>(tokenHandler.WriteToken(token), "generated access token");
        }

        public IDataResult<string> CreateRefreshToken(string id)
        {
            var secret = "fd6d2c6fa1c2ff8959f84c81aa0cc14345162172d3d6598347696089db6918e1";
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMonths(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = tokenHandler.WriteToken(token);

            var refreshKey = $"refresh_token:{id}";
            const int ONE_MONTH = 24 * 60 * 60 * 30; // seconds
            _redisService.SetAsync(refreshKey, refreshToken, ONE_MONTH);

            return new SuccessDataResult<string>(refreshToken, "generated refresh token");
        }
    }
}
