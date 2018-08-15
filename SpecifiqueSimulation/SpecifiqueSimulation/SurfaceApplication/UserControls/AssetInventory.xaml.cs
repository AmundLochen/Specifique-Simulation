using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Inventory.xaml
    /// </summary>
    public partial class AssetInventory
    {
        private List<Asset> _assetInventoryList;

        public AssetInventory()
        {
            Assets = new List<AssetModel>();
            AssetInventories = new List<AssetInventoryModel>();
            InitializeComponent();
            _assetInventoryList = new List<Asset>();
            SurfaceListBox1.DataContext = _assetInventoryList;
        }

        public List<AssetModel> Assets { get; set; }
        public List<AssetInventoryModel> AssetInventories { get; set; }


        public int AssetId
        {
            get
            {
                object o = SurfaceListBox1.SelectedValue;
                var a = (Asset) o;
                return a.Id;
            }
        }

        public event EventHandler SurfaceListBox1SelectedValueChanged;

        /// <summary>
        ///     Gets asset.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AssetModel GetAssetModel(int id)
        {
            foreach (AssetModel am in Assets)
                if (am.Id == id)
                    return am;

            return null;
        }

        /// <summary>
        ///     Edits asset.
        /// </summary>
        /// <param name="asset"></param>
        public void EditAssetModel(AssetModel asset)
        {
            foreach (AssetModel a in Assets)
                if (a.Id == asset.Id)
                {
                    a.Price = asset.Price;
                }
        }

        /// <summary>
        ///     Gets a group's assets.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<AssetModel> GetAssetModels(int groupId)
        {
            var ams = new List<AssetModel>();
            foreach (AssetModel am in Assets)
                foreach (AssetInventoryModel aim in AssetInventories)
                    if (am.Id == aim.AssetId && aim.GroupId == groupId && aim.Share != 0)
                        ams.Add(am);
            return ams;
        }

        /// <summary>
        ///     Adds new asset inventory.
        /// </summary>
        /// <param name="assetInventory"></param>
        public void AddAssetInventoryModel(AssetInventoryModel assetInventory)
        {
            AssetInventories.Add(assetInventory);
            AddAssets(Assets, AssetInventories);
        }

        /// <summary>
        ///     Edits asset inventory.
        /// </summary>
        /// <param name="assetInventory"></param>
        public void EditAssetInventoryModel(AssetInventoryModel assetInventory)
        {
            foreach (AssetInventoryModel aim in AssetInventories)
                if (aim.AssetId == assetInventory.AssetId && aim.GroupId == assetInventory.GroupId)
                    aim.Share = assetInventory.Share;

            AddAssets(Assets, AssetInventories);
        }

        /// <summary>
        ///     Gets an asset's asset inventories.
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public List<AssetInventoryModel> GetAssetInventoryModels(int assetId)
        {
            var aims = new List<AssetInventoryModel>();

            foreach (AssetInventoryModel aim in AssetInventories)
                if (aim.AssetId == assetId && aim.Share != 0)
                    aims.Add(aim);

            return aims;
        }

        /// <summary>
        ///     Gets a group's asset inventories.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<AssetInventoryModel> GetGroupsAssetInventoryModels(int groupId)
        {
            var aims = new List<AssetInventoryModel>();

            foreach (AssetInventoryModel aim in AssetInventories)
                if (aim.GroupId == groupId && aim.Share != 0)
                    aims.Add(aim);

            return aims;
        }

        /// <summary>
        ///     Checks if asset inventory exists.
        /// </summary>
        /// <param name="assetInventory"></param>
        /// <returns></returns>
        public Boolean AssetInventoryExists(AssetInventoryModel assetInventory)
        {
            foreach (AssetInventoryModel aim in AssetInventories)
            {
                if (assetInventory.AssetId == aim.AssetId && assetInventory.GroupId == aim.GroupId)
                    return true;
            }

            return false;
        }


        public void SetSurfaceListBox1SelectedValue(int id)
        {
            int index = 0;
            //Boolean isInInventory = false;

            foreach (Asset asset in SurfaceListBox1.Items)
            {
                if (asset.Id == id)
                {
                    SurfaceListBox1.SelectedIndex = index;
                    break;
                }
                index++;
            }
        }

        public void AddAssets(List<AssetModel> ams, List<AssetInventoryModel> aims)
        {
            int share = 0;
            var newAssets = new List<Asset>();

            foreach (AssetModel am in ams)
            {
                foreach (AssetInventoryModel aim in aims)
                {
                    if (am.Id == aim.AssetId && aim.Share != 0)
                    {
                        share = aim.Share;
                        break;
                    }
                }
                newAssets.Add(new Asset(am.Id, am.Name, share));
                share = 0;
            }


            _assetInventoryList = newAssets;
            _assetInventoryList = newAssets.OrderBy(o => o.Name).ToList();
            TrySetDataContext();
        }

        private void TrySetDataContext()
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                SetDataContext();
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(SetDataContext), DispatcherPriority.Normal);
            }
        }

        private void SetDataContext()
        {
            SurfaceListBox1.DataContext = _assetInventoryList;
        }

        private void SurfaceButton_Click(object sender, RoutedEventArgs e) // Hides or shows assets in inventory
        {
            if (SurfaceListBox1.Visibility != Visibility.Collapsed)
                SurfaceListBox1.Visibility = Visibility.Collapsed;

            else
                SurfaceListBox1.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Event for when asset is selected.
        /// </summary>
        private void surfaceListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EventHandler handler = SurfaceListBox1SelectedValueChanged;
            if (handler != null)
                handler(sender, e);
        }

        private class Asset
        {
            public Asset(int i, string n, int s)
            {
                Id = i;
                Name = n;
                Share = s;
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public int Share { get; set; }
        }
    }
}