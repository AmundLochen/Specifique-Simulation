using System;

namespace SpecifiqueSimulationServer
{
    /// <summary>
    ///     Contains time logic and values
    /// </summary>
    public class GameClock
    {
        private DateTime _gameTime;
        private DateTime _realTime;
        private TimeSpan _timeSpeed;

        public GameClock()
        {
            DateTime now = DateTime.Now;
            _realTime = now;
            _gameTime = now;
            _timeSpeed = TimeSpan.FromHours(24);
        }

        public DateTime GameTime
        {
            get
            {
                TimeSpan durationSinceSet = DateTime.Now - _realTime;
                TimeSpan gameTimeSpan = TimeSpan.FromSeconds(durationSinceSet.TotalSeconds*_timeSpeed.TotalSeconds);
                return _gameTime + gameTimeSpan;
            }
        }

        /// <summary>
        ///     Sets date
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="speed">Speed</param>
        public void SetDate(DateTime date, TimeSpan speed)
        {
            _realTime = DateTime.Now;
            _gameTime = date;
            _timeSpeed = speed;
        }
    }
}