using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IEmailService
    {
        Task SendEmail(EmailRequest emailRequest);
    }

    public class EmailRequest
    {
        public string ToUser { get; set; }
        public string Code { get; set; }
        public int Type { get; set; }
    }
}
