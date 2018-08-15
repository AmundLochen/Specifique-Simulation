using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Asset.xaml
    /// </summary>
    public partial class Asset
    {
        //Stored variables
        private List<Owner> _owners;

        public Asset()
        {
            InitializeComponent();
            _owners = new List<Owner>();
            ownersListBox.DataContext = _owners;
        }

        public AssetModel Assets { get; private set; }
        private List<GroupModel> Groups { get; set; }
        private List<AssetInventoryModel> AssetInventories { get; set; }

        //Triggering events
        // public event EventHandler exitButtonPressed;
        public event EventHandler TradeButtonPressed;

        // Uses the Dispatcher.VerifyAccess method to determine if  
        // the calling thread has access to the thread the UI object is on. 
        public void TryToUpdate(AssetModel a, List<GroupModel> gms, List<AssetInventoryModel> aims)
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                SetAsset(a, gms, aims);
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                Assets = a;
                Groups = gms;
                AssetInventories = aims;

                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(Update), DispatcherPriority.Normal);
            }
        }

        private void Update()
        {
            string packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + Assets.Name + ".png";
            try
            {
                assetIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            }

            catch (NullReferenceException)
            {
                try
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + Assets.Name + ".jpg";
                    assetIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }

                catch (NullReferenceException)
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/specifique.png";
                    assetIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }
            }

            _owners = new List<Owner>();
            assetNameText.Text = Assets.Name;
            assetValueText.Text = Assets.Price.ToString();
            assetDescriptionText.Text = Assets.Description;

            foreach (AssetInventoryModel aim in AssetInventories)
                if (aim.Share > 0)
                    foreach (GroupModel gm in Groups)
                        if (aim.GroupId == gm.Id)
                            _owners.Add(new Owner(gm.Name, aim.Share));

            //owners = owners.OrderBy(o => o.share).ToList();
            ownersListBox.DataContext = _owners;
        }

        public void SetAsset(AssetModel a, List<GroupModel> gms, List<AssetInventoryModel> aims)
        {
            string packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + a.Name + ".png";

            try
            {
                assetIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            }

            catch (NullReferenceException)
            {
                try
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + a.Name + ".jpg";
                    assetIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }

                catch (NullReferenceException)
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/specifique.png";
                    assetIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }
            }

            Assets = a;

            _owners = new List<Owner>();
            assetNameText.Text = a.Name;
            assetValueText.Text = a.Price.ToString();
            assetDescriptionText.Text = a.Description;

            foreach (AssetInventoryModel aim in aims)
                if (aim.Share > 0)
                    foreach (GroupModel gm in gms)
                        if (aim.GroupId == gm.Id)
                            _owners.Add(new Owner(gm.Name, aim.Share));
            ownersListBox.DataContext = _owners;
        }

        private void trade_Click(object sender, RoutedEventArgs e)
        {
            EventHandler handler = TradeButtonPressed;

            if (handler != null)
                handler(sender, e);
        }

        public class Owner
        {
            public Owner(string n, int s)
            {
                Name = n;
                Share = s;
            }

            public string Name { get; set; }
            public int Share { get; set; }
        }
    }
}