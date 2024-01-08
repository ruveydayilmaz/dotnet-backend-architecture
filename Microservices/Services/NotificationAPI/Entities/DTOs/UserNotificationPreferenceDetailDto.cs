using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class UserNotificationPreferenceDetailDto : IDto
    {
        public int NotificationId { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; } // "admin", "user" etc
        public string NotificationType { get; set; } // "alert", "info", "reminder" etc
        public bool IsEnabled { get; set; }
    }
}
