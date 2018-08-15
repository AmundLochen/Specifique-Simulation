using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Gameboard.xaml
    /// </summary>
    public partial class Gameboard
    {
        //Stored group id
        public Gameboard()
        {
            InitializeComponent();
        }

        public int Id { get; set; }

        //Event Trigger
        public event EventHandler GameboardSelectedValueChanged;

        public void SetGameboardAsset(int uid)
        {
            UIElement element = GetByUid(this, uid.ToString());
            var b = (Button) element;
            Id = int.Parse(b.Uid);
            AnimateButton(b);
        }

        private static UIElement GetByUid(DependencyObject rootElement, string uid)
        {
            foreach (UIElement element in LogicalTreeHelper.GetChildren(rootElement).OfType<UIElement>())
            {
                if (element.Uid == uid)
                    return element;
                UIElement resultChildren = GetByUid(element, uid);
                if (resultChildren != null)
                    return resultChildren;
            }
            return null;
        }

        private void button_Click(object sender, EventArgs e)
        {
            EventHandler handler = GameboardSelectedValueChanged;
            var b = (Button) sender;
            Id = int.Parse(b.Uid);

            if (handler != null)
                handler(sender, e);

            AnimateButton(b);
        }

        private static void AnimateButton(Button b)
        {
            var d = new Duration(TimeSpan.FromMilliseconds(500));
            var ani = new DoubleAnimation(b.Opacity, 0.4, d);
            ani.AutoReverse = true;

            var sb = new Storyboard();
            sb.Children.Add(ani);

            Storyboard.SetTarget(ani, b);
            Storyboard.SetTargetProperty(ani, new PropertyPath("Opacity"));
            ani.FillBehavior = FillBehavior.Stop;
            sb.Begin();
        }
    }
}