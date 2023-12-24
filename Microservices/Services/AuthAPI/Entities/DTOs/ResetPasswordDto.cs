using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class ResetPasswordDto : IDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsEmail { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
