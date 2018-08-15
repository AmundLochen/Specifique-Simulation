using System;
using System.Windows.Threading;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Gameboard.xaml
    /// </summary>
    public partial class Clock
    {
        private string _ny;

        public Clock()
        {
            InitializeComponent();
        }

        public void SetTime(DateTime time)
        {
            _ny = time.ToShortDateString();
        }

        public void TryShowTime()
        {
            try
            {
                Dispatcher.VerifyAccess();
                lblClock.Content = _ny;
            }
            catch (InvalidOperationException)
            {
                Dispatcher.BeginInvoke(new Action(ShowTime), DispatcherPriority.Normal);
            }
        }

        private void ShowTime()
        {
            lblClock.Content = _ny;
        }
    }
}