using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for YourMoneyStream.xaml
    /// </summary>
    public partial class YourMoneyStream
    {
        //Storing variables
        public double MoneyValues { get; set; }
        public String Group { get; set; }
        private bool orientation;

        //EventHandler
        public event EventHandler ExpandMenu;
        
        public YourMoneyStream()
        {
            MoneyValues = 0;
            Group = "";
            orientation = false;
            InitializeComponent();
        }

        public void SetGroup()
        {
            Dispatcher.Invoke((Action) (() => { mGroup.Text = Group; }));
        }

        public void UpdateValues(String chngMoney)
        {
            Dispatcher.Invoke((Action) (() =>
                {
                    MoneyValues = double.Parse(chngMoney);
                    mValue.Text = String.Format("{0:C}", MoneyValues);
                }));
        }

        private void ExpandMenu_Click(object sender, EventArgs e)
        {
            if (!orientation)
            {
                orientation = true;
                (btnexpandMenu.Content as Image).Source = new BitmapImage(new Uri("/Images/arrowDown.png", UriKind.Relative));
            }
            else
            {
                orientation = false;
                (btnexpandMenu.Content as Image).Source = new BitmapImage(new Uri("/Images/arrow.png", UriKind.Relative));
            }
            
            EventHandler handler = ExpandMenu;
            if (handler != null)
                handler(sender, e);
        }
    }
}