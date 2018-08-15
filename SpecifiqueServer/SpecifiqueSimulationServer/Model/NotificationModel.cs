using System;

namespace Model
{
    public class NotificationModel
    {
        public NotificationModel()
        {
        }

        public NotificationModel(int i, int nTo, string s, string txt, DateTime t)
        {
            Id = i;
            NotificTo = nTo;
            Subject = s;
            Text = txt;
            Time = t;
        }

        public int Id { get; set; }
        public int NotificTo { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}