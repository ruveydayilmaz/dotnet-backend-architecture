using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class UserNotificationPreference : IEntity
    {
        [Key]
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string UserType { get; set; } // "admin", "user" etc
        public string NotificationType { get; set; } // "alert", "info", "reminder" etc
        public bool IsEnabled { get; set; }
    }
}
