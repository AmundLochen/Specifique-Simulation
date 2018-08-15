using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    /// <summary>
    ///     Contains notification logic
    /// </summary>
    public static class NotificationLogic
    {
        private static readonly NotificationDal NotificationDal = new NotificationDal();

        /// <summary>
        ///     Gets all notifications
        /// </summary>
        /// <returns>Notifications</returns>
        public static List<NotificationModel> GetAllNotifics()
        {
            return NotificationDal.GetAllNotifics();
        }

        /// <summary>
        ///     Adds new notification to DB
        /// </summary>
        /// <param name="n">Notification</param>
        /// <returns>Notification</returns>
        public static NotificationModel NewNotific(NotificationModel n)
        {
            return NotificationDal.NewNotific(n);
        }

        /// <summary>
        ///     Gets notifications
        /// </summary>
        /// <param name="teamId">To or from id</param>
        /// <returns>Notifications</returns>
        public static List<NotificationModel> GetNotifications(int teamId)
        {
            return NotificationDal.GetNotifications(teamId);
        }
    }
}