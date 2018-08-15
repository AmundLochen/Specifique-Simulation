using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Notifications.xaml
    /// </summary>
    public partial class Notifications
    {
        //Storing variables

        //Event handlers
        public EventHandler ShowNotificationNotification;


        public Notifications()
        {
            notifications = new List<NotificationModel>();
            InitializeComponent();
        }

        private List<NotificationModel> notifications { get; set; }

        private void ShowNotificationIcon(object recieved, EventArgs e)
        {
            EventHandler handler = ShowNotificationNotification;
            if (handler != null)
                handler(recieved, e);
        }

        public void AddNotification(NotificationModel notification)
        {
            notification.Time = notification.Time.Date;
            notifications.Add(notification);
            notifications = notifications.OrderByDescending(o => o.Time).ToList();
            TrySetDataContext();
            ShowNotificationIcon("test", null);
        }

        private void TrySetDataContext()
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                SetDataContext();
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(SetDataContext), DispatcherPriority.Normal);
            }
        }

        private void SetDataContext()
        {
            listBoxNotification.DataContext = notifications;
            listBoxNotification.Items.Refresh();
        }
    }
}