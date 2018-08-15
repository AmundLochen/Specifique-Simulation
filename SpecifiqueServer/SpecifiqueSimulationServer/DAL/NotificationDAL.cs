using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    /// <summary>
    ///     Contains notification dal
    /// </summary>
    public class NotificationDal
    {
        private readonly DBlinqDataContext _db = new DBlinqDataContext();

        /// <summary>
        ///     Gets all notifications
        /// </summary>
        /// <returns>Notifications</returns>
        public List<NotificationModel> GetAllNotifics()
        {
            List<NotificationModel> notifics = (from n in _db.notifics
                                                select new NotificationModel
                                                    {
                                                        Id = n.Id,
                                                        NotificTo = n.teamId,
                                                        Subject = n.notificSubject,
                                                        Text = n.notificText,
                                                        Time = (DateTime) n.notificTime
                                                    }).ToList();

            return notifics;
        }

        /// <summary>
        ///     Adds new notification to DB
        /// </summary>
        /// <param name="n">Notification</param>
        /// <returns>Notification</returns>
        public NotificationModel NewNotific(NotificationModel n)
        {
            var newNotific = new notific
                {
                    notificSubject = n.Subject,
                    teamId = n.NotificTo,
                    notificText = n.Text,
                    notificTime = n.Time
                };

            _db.notifics.InsertOnSubmit(newNotific);
            try
            {
                _db.SubmitChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            int id = (from not in _db.notifics
                      orderby not.Id descending
                      select not.Id).First();

            NotificationModel outgoing = GetNotific(id);
            return outgoing;
        }

        /// <summary>
        ///     Gets notification
        /// </summary>
        /// <param name="id">Notification id</param>
        /// <returns>Notification</returns>
        private NotificationModel GetNotific(int id)
        {
            NotificationModel nModel = (from notific n in _db.notifics
                                        where n.Id == id
                                        select new NotificationModel
                                            {
                                                Id = n.Id,
                                                NotificTo = n.teamId,
                                                Subject = n.notificSubject,
                                                Text = n.notificText,
                                                Time = (DateTime) n.notificTime
                                            }).ToList().Last();
            return nModel;
        }

        public List<NotificationModel> GetNotifications(int teamId)
        {
            List<NotificationModel> notifications = (from notific in _db.notifics
                                                     where notific.teamId == teamId
                                                     orderby notific.Id
                                                     select new NotificationModel
                                                         {
                                                             Id = notific.Id,
                                                             NotificTo = notific.teamId,
                                                             Subject = notific.notificSubject,
                                                             Text = notific.notificSubject,
                                                             Time = (DateTime) notific.notificTime
                                                         }).ToList();

            return notifications;
        }
    }
}