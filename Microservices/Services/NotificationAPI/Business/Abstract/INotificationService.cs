using Core.Utilities.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Concrete;

namespace Business.Abstract
{
    public interface INotificationService
    {
        IDataResult<List<UserNotificationPreference>> GetUserNotificationPreferences(string userId);
        //IDataResult<UserNotificationPreference> AddNotificationTypeToUser(int notificationTypeId);
        //IResult Add(UserNotificationPreference userNotificationPreference);
    }
}
