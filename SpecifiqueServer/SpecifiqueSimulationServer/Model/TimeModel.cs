using System;
using System.Collections;

namespace Model
{
    public class TimeModel : IEnumerable
    {
        public DateTime CurrentTime { get; set; }
        public DateTime EndTime { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return CurrentTime;
            yield return EndTime;
        }
    }
}