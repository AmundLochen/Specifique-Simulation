using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Item.xaml
    /// </summary>
    public partial class Item
    {
        //Storing variables
        private List<Owner> owners;

        public Item()
        {
            InitializeComponent();
            owners = new List<Owner>();
            ownersListBox.DataContext = owners;
        }

        public ItemModel ItemModel { get; set; }
        private List<GroupModel> GroupModels { get; set; }
        private List<ItemInventoryModel> ItemInventoryModels { get; set; }

        //Event Handlers
        //public event EventHandler exitButtonPressed;
        public event EventHandler TradeButtonPressed;

        // Uses the Dispatcher.VerifyAccess method to determine if  
        // the calling thread has access to the thread the UI object is on. 
        public void TryToUpdate(ItemModel i, List<GroupModel> gms, List<ItemInventoryModel> iims)
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                SetItem(i, gms, iims);
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                ItemModel = i;
                GroupModels = gms;
                ItemInventoryModels = iims;

                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(Update), DispatcherPriority.Normal);
            }
        }

        private void Update()
        {
            string packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + ItemModel.Name + ".png";

            try
            {
                itemIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            }

            catch (NullReferenceException)
            {
                try
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + ItemModel.Name + ".jpg";
                    itemIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }

                catch (NullReferenceException)
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/specifique.png";
                    itemIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }
            }

            owners = new List<Owner>();
            itemNameText.Text = ItemModel.Name;
            itemValueText.Text = ItemModel.Value.ToString();
            itemDescriptionText.Text = ItemModel.Description;

            foreach (ItemInventoryModel iim in ItemInventoryModels)
                if (iim.Quantity > 0)
                    foreach (GroupModel gm in GroupModels)
                        if (iim.GroupId == gm.Id)
                            owners.Add(new Owner(gm.Name, iim.Quantity));

            //owners = owners.OrderBy(o => o.name).ToList();
            ownersListBox.DataContext = owners;
        }

        public void SetItem(ItemModel im, List<GroupModel> gms, List<ItemInventoryModel> iims)
        {
            string packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + im.Name + ".png";

            try
            {
                itemIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            }

            catch (NullReferenceException)
            {
                try
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/" + im.Name + ".jpg";
                    itemIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }

                catch (NullReferenceException)
                {
                    packUri = "pack://application:,,,/SurfaceApplication;component/Images/specifique.png";
                    itemIcon.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                }
            }

            ItemModel = im;
            owners = new List<Owner>();
            itemNameText.Text = im.Name;
            itemValueText.Text = im.Value.ToString();
            itemDescriptionText.Text = im.Description;

            foreach (ItemInventoryModel iim in iims)
                if (iim.Quantity > 0)
                    foreach (GroupModel gm in gms)
                        if (iim.GroupId == gm.Id)
                            owners.Add(new Owner(gm.Name, iim.Quantity));

            owners = owners.OrderBy(o => o.Quantity).ToList();
            ownersListBox.DataContext = owners;
        }

        /* private void Exit_Click(object sender, RoutedEventArgs e)
        {
            EventHandler handler = exitButtonPressed;

            if (handler != null)
                handler(sender, e);
        }*/

        private void Trade_Click(object sender, RoutedEventArgs e)
        {
            EventHandler handler = TradeButtonPressed;
            if (handler != null)
                handler(sender, e);
        }

        private class Owner
        {
            public Owner(string n, double q)
            {
                Name = n;
                Quantity = q;
            }

            public string Name { get; set; }
            public double Quantity { get; set; }
        }
    }
}