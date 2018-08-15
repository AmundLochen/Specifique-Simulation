using System;
using System.Collections;

namespace Model
{
    public class MessageModel : IEnumerable
    {
        public MessageModel(int i, int fi, int ti, int p, string sub, string txt, DateTime t)
        {
            Id = i;
            FromId = fi;
            ToId = ti;
            ParentId = p;
            Subject = sub;
            Text = txt;
            Time = t;
        }

        public MessageModel()
        {
        }

        public int Id { get; set; }
        public int FromId { get; set; }
        public string FromName { get; set; }
        public int ToId { get; set; }
        public string ToName { get; set; }
        public int ParentId { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return Id;
            yield return FromId;
            yield return ToId;
            yield return ParentId;
            yield return Subject;
            yield return Text;
            yield return Time;
        }
    }
}