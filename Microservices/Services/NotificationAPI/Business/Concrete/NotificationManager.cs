using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class NotificationManager : INotificationService
    {
        INotificationDal _notificationDal;

        public NotificationManager(INotificationDal notificationDal)
        {
            _notificationDal = notificationDal;
        }

        //public IDataResult<UserNotificationPreference> AddNotificationTypeToUser(int notificationTypeId)
        //{
        //    throw new NotImplementedException();
        //}

        public IDataResult<List<UserNotificationPreference>> GetUserNotificationPreferences()
        {
            return new SuccessDataResult<List<UserNotificationPreference>>(_notificationDal.GetAll(), Messages.PreferencesListed);
        }
    }
}
