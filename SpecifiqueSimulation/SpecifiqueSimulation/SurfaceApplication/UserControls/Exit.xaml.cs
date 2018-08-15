using System.Windows;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Exit.xaml
    /// </summary>
    public partial class Exit
    {
        public Exit()
        {
            InitializeComponent();
        }

        private void surfaceButton1_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
                window.Close();
        }
    }
}