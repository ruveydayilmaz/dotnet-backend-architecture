using Business.Abstract;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebAPI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IVerificationService _verificationService;
        private readonly ITokenService _tokenService;
        private readonly IRedisService _redisService;

        public AuthController(IUserService userService, IEmailService emailService, IVerificationService verificationService, ITokenService tokenService, IRedisService redisService)
        {
            _userService = userService;
            _emailService = emailService;
            _verificationService = verificationService;
            _tokenService = tokenService;
            _redisService = redisService;
        }

        [HttpPost("send-otp-to-email")]
        public IActionResult SendOtpToEmail([FromBody] SendOtpRequestDto request)
        {
            var existingUser = _userService.GetByEmail(request.Email);

            if (existingUser != null && existingUser.Data != null && existingUser.Data.Verified.IsVerified)
            {
                return NotFound(new { message = "AlreadyVerified" });
            }

            Random random = new Random();
            int verificationCode = random.Next(100000, 999999);

            var result = _redisService.SaveVerificationCode(request.Email, verificationCode.ToString());


            //_emailService.SendEmail(new EmailRequest  -> i don't have smtp
            //{
            //    ToUser = request.Email,
            //    Code = result.Data,
            //    Type = 0
            //});

            return Ok(result);
        }

        [HttpPost("verify-phone-or-email")]
        public IActionResult VerifyPhoneOrEmail([FromBody] VerifyPhoneOrEmailDto request)
        {

            FilterOptions filter = null;

            if (request.IsEmail)
            {
                filter = new FilterOptions { Email = request.Email };
            }
            else
            {
                filter = new FilterOptions { PhoneNumber = request.PhoneNumber };
            }

            var existingUser = _userService.GetOneByEmailOrPhoneNumber(filter, request.IsEmail);

            if (existingUser != null && existingUser.Data != null && existingUser.Data.Verified.IsVerified)
            {
                return NotFound(new { message = "AlreadyVerified" });
            }

            var emailOrPhone = request.IsEmail ? request.Email : request.PhoneNumber;
            string key = $"verification_code:{emailOrPhone}";
            var isCorrect = _verificationService.CheckVerificationCode(key, request.Code);

            if (!isCorrect.Data)
            {
                return BadRequest(new { message = "InvalidCode" });
            }

            var status = _verificationService.SetVerified(emailOrPhone);

            var responseData = new
            {
                verified = status,
            };

            return Ok(new { message = "SuccessfullyVerified", data = responseData });

        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {

            FilterOptions filter = null;

            if (request.IsEmail)
            {
                filter = new FilterOptions { Email = request.Email };
            }
            else
            {
                filter = new FilterOptions { PhoneNumber = request.PhoneNumber };
            }

            var existingUser = _userService.GetOneByEmailOrPhoneNumber(filter, request.IsEmail);

            if (existingUser != null && existingUser.Data != null)
            {
                return NotFound(new { message = "Already Registered" });
            }

            var emailOrPhone = request.IsEmail ? request.Email : request.PhoneNumber;
            string key = $"{emailOrPhone}";
            var isCorrect = _verificationService.CheckVerificationCode(key, "true");

            if (!isCorrect.Data)
            {
                return BadRequest(new { message = "InvalidCode" });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            User user = new User
            {
                Email = request.Email,
                Password = hashedPassword,
                PhoneNumber = request.PhoneNumber,
                Location = new Location
                {
                    City = request.Location.City,
                    Country = request.Location.Country
                },
                Verified = new VerifiedInfo
                {
                    IsVerified = true,
                    VerifiedWith = request.IsEmail ? "email" : "phone",
                },
            };

            var createdUser = _userService.Add(user);

            var accessToken = _tokenService.CreateAccessToken(createdUser.Data.Id);
            var refreshToken = _tokenService.CreateRefreshToken(createdUser.Data.Id);

            var responseData = new
            {
                User = user,
                AccessToken = accessToken.Data,
                RefreshToken = refreshToken.Data
            };

            return Ok(new
            {
                message = "Successfully Registered",
                data = responseData
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {

            FilterOptions filter = null;

            if (request.IsEmail)
            {
                filter = new FilterOptions { Email = request.Email };
            }
            else
            {
                filter = new FilterOptions { PhoneNumber = request.PhoneNumber };
            }

            var existingUser = _userService.GetOneByEmailOrPhoneNumber(filter, request.IsEmail);

            if (existingUser.Data == null)
            {
                return NotFound(new { message = "Not Registered" });
            }

            var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Data.Password);

            if (!isPasswordCorrect)
            {
                return NotFound(new { message = "Invalid Credentials" });
            }

            var accessToken = _tokenService.CreateAccessToken(existingUser.Data.Id);
            var refreshToken = _tokenService.CreateRefreshToken(existingUser.Data.Id);

            var responseData = new
            {
                User = existingUser.Data,
                AccessToken = accessToken.Data,
                RefreshToken = refreshToken.Data
            };

            return Ok(new
            {
                message = "Successfully Login",
                data = responseData
            });
        }

        [HttpPost("refresh-access-token")]
        public IActionResult RefreshAccessToken([FromBody] RefreshAccessTokenDto request)
        {

            var userId = _tokenService.VerifyRefreshToken(request.RefreshToken);

            var existingUser = _userService.GetById(userId.Data);

            if (existingUser.Data == null)
            {
                return NotFound(new { message = "Not Registered" });
            }

            var accessToken = _tokenService.CreateAccessToken(existingUser.Data.Id);

            var responseData = new
            {
                AccessToken = accessToken.Data
            };

            return Ok(new
            {
                message = "Successfully refreshed access token",
                data = responseData
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshAccessTokenDto request)
        {

            var userId = _tokenService.VerifyRefreshToken(request.RefreshToken);
            var isDeleted = _redisService.DeleteKey($"refresh_token:{request.RefreshToken}");

            var responseData = new
            {
                IsDeleted = isDeleted.Data,
                UserId = userId.Data,
            };

            return Ok(new
            {
                message = "Successfully logged out",
                data = responseData
            });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            FilterOptions filter = null;

            if (request.IsEmail)
            {
                filter = new FilterOptions { Email = request.Email };
            }
            else
            {
                filter = new FilterOptions { PhoneNumber = request.PhoneNumber };
            }

            var existingUser = _userService.GetOneByEmailOrPhoneNumber(filter, request.IsEmail);

            if (existingUser.Data == null)
            {
                return NotFound(new { message = "Not Registered" });
            }

            Random random = new Random();
            int verificationCode = random.Next(100000, 999999);

            string emailOrPhone = request.IsEmail ? request.Email : request.PhoneNumber;
            string key = $"forgot:{emailOrPhone}";
            const int THREE_MIN = 60 * 3; // seconds
            var result = _redisService.SetAsync(key, verificationCode.ToString(), THREE_MIN);

            if (request.IsEmail)
            {
                //_emailService.SendEmail(new EmailRequest  -> i don't have smtp
                //{
                //    ToUser = request.Email,
                //    Code = result.Data,
                //    Type = 0
                //});
            }
            else
            {
                // send sms
            }

            return Ok(new
            {
                code = verificationCode,
                message = "Successfully sent code for forgot password"
            });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto request)
        {
            string emailOrPhone = request.IsEmail ? request.Email : request.PhoneNumber;
            string key = $"forgot:{emailOrPhone}";

            var verificationCode = _redisService.GetAsync(key);

            if(request.Code != verificationCode.Data)
            {
                return BadRequest(new { message = "Invalid Code" });
            }

            FilterOptions filter = null;

            if (request.IsEmail)
            {
                filter = new FilterOptions { Email = request.Email };
            }
            else
            {
                filter = new FilterOptions { PhoneNumber = request.PhoneNumber };
            }

            var existingUser = _userService.GetOneByEmailOrPhoneNumber(filter, request.IsEmail);

            if (existingUser.Data == null)
            {
                return NotFound(new { message = "Not Registered" });
            }

            var hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            User user = new User
            {
                Id = existingUser.Data.Id,
                Password = hashedNewPassword
            };

            _userService.UpdatePassword(user, hashedNewPassword);

            return Ok(new
            {
                message = "Successfully updated password"
            });
        }
    }
}
