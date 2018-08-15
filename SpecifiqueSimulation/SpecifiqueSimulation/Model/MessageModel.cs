using System;
using System.ComponentModel;

namespace Model
{
    public class MessageModel
    {
        public MessageModel(int i, int f, int t,string fn, string tn, int p, string s, string txt, DateTime tm)
        {
            Id = i;
            From = f;
            To = t;
            FromName = fn;
            ToName = tn;
            ParentId = p;
            Subject = s;
            Text = txt;
            Time = tm;
        }

        public int Id { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        public int ParentId { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }


        //testing av oppdatering av usercontroll inbox
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}