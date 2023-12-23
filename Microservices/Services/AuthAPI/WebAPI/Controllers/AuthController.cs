using Business.Abstract;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IVerificationService _verificationService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, IEmailService emailService, IVerificationService verificationService, ITokenService tokenService)
        {
            _userService = userService;
            _emailService = emailService;
            _verificationService = verificationService;
            _tokenService = tokenService;
        }

        [HttpPost("send-otp-to-email")]
        public IActionResult SendOtpToEmail([FromBody] SendOtpRequestDto request)
        {
            var existingUser = _userService.FindByEmail(request.Email);

            if (existingUser != null && existingUser.Data != null && existingUser.Data.Verified.IsVerified)
            {
                return NotFound(new { message = "AlreadyVerified" });
            }

            var code = _userService.GenerateVerificationCode(request.Email);
            //_emailService.SendEmail(new EmailRequest  -> i don't have smtp
            //{
            //    ToUser = request.Email,
            //    Code = code.Data,
            //    Type = 0
            //});

            return Ok(code);
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

            var existingUser = _userService.GetOne(filter, request.IsEmail);

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

            var existingUser = _userService.GetOne(filter, request.IsEmail);

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
            var refreshToken =  _tokenService.CreateRefreshToken(createdUser.Data.Id);

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

            var existingUser = _userService.GetOne(filter, request.IsEmail);

            if (existingUser.Data == null)
            {
                return NotFound(new { message = "Not Registered" });
            }

            var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Data.Password);

            if(!isPasswordCorrect)
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
    }
}
