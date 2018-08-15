using System;

namespace Model
{
    public class NotificationModel
    {
        public NotificationModel(int i, string s, string txt, DateTime tm, int t)
        {
            Id = i;
            Subject = s;
            Text = txt;
            Time = tm;
            To = t;
        }

        public int Id { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public int To { get; set; }
    }
}