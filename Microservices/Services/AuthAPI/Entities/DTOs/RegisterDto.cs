using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class RegisterDto : IDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public LocationDto Location { get; set; }
        public bool IsEmail { get; set; }
    }

    public class LocationDto
    {
        public string City { get; set; }
        public string Country { get; set; }
    }
}
