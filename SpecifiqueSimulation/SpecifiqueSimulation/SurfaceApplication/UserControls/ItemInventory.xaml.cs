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
    ///     Interaction logic for ItemInventory.xaml
    /// </summary>
    public partial class ItemInventory
    {
        //Storing variables
        private List<Item> itemInventoryList;

        public ItemInventory()
        {
            Items = new List<ItemModel>();
            ItemInventories = new List<ItemInventoryModel>();
            InitializeComponent();
            itemInventoryList = new List<Item>();
            //addItems();
            surfaceListBox2.DataContext = itemInventoryList;
        }

        public List<ItemModel> Items { get; set; }
        public List<ItemInventoryModel> ItemInventories { get; set; }

        //Event handlers

        public int ItemId
        {
            get
            {
                object o = surfaceListBox2.SelectedValue;
                var i = (Item) o;
                return i.Id;
            }
        }

        public event EventHandler SurfaceListBox2SelectedValueChanged;

        /// <summary>
        ///     Gets item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ItemModel GetItemModel(int id)
        {
            foreach (ItemModel im in Items)
                if (im.Id == id)
                    return im;

            return null;
        }

        /// <summary>
        ///     Gets a group's items.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<ItemModel> GetItemModels(int groupId)
        {
            var ims = new List<ItemModel>();
            foreach (ItemModel im in Items)
                foreach (ItemInventoryModel iim in ItemInventories)
                    if (im.Id == iim.ItemId && iim.GroupId == groupId && iim.Quantity != 0)
                        ims.Add(im);
            return ims;
        }

        /// <summary>
        ///     Gets an item's item inventories.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public List<ItemInventoryModel> GetItemInventoryModels(int itemId)
        {
            var iims = new List<ItemInventoryModel>();
            foreach (ItemInventoryModel iim in ItemInventories)
                if (iim.ItemId == itemId && iim.Quantity != 0)
                    iims.Add(iim);
            return iims;
        }

        /// <summary>
        ///     Gets a group's item inventories.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<ItemInventoryModel> GetGroupsItemInventoryModels(int groupId)
        {
            var iims = new List<ItemInventoryModel>();

            foreach (ItemInventoryModel iim in ItemInventories)
                if (iim.GroupId == groupId && iim.Quantity != 0)
                    iims.Add(iim);

            return iims;
        }

        /// <summary>
        ///     Checks if item inventory exists.
        /// </summary>
        /// <param name="itemInventory"></param>
        /// <returns></returns>
        public Boolean ItemInventoryExists(ItemInventoryModel itemInventory)
        {
            foreach (ItemInventoryModel iim in ItemInventories)
            {
                if (itemInventory.ItemId == iim.ItemId && itemInventory.GroupId == iim.GroupId)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     Edits item price
        /// </summary>
        /// <param name="item">Item</param>
        public void EditItemPrice(ItemModel item)
        {
            var newItems = new List<ItemModel>();

            foreach (ItemModel i in Items)
            {
                if (i.Id == item.Id)
                    newItems.Add(item);
                else
                    newItems.Add(i);
            }

            Items = newItems;
        }

        /// <summary>
        ///     Edits item inventory.
        /// </summary>
        /// <param name="itemInventory"></param>
        public void EditItemInventoryModel(ItemInventoryModel itemInventory)
        {
            foreach (ItemInventoryModel iim in ItemInventories)
                if (iim.ItemId == itemInventory.ItemId && iim.GroupId == itemInventory.GroupId)
                    iim.Quantity = itemInventory.Quantity;
        }

        /// <summary>
        ///     Adds item inventory.
        /// </summary>
        /// <param name="itemInventory"></param>
        public void AddItemInventoryModel(ItemInventoryModel itemInventory)
        {
            ItemInventories.Add(itemInventory);
        }


        public void AddItems(List<ItemModel> ims, List<ItemInventoryModel> iims)
        {
            double quantity = 0;
            var newItems = new List<Item>();

            foreach (ItemModel im in ims)
            {
                foreach (ItemInventoryModel iim in iims)
                {
                    if (im.Id == iim.ItemId && iim.Quantity != 0)
                    {
                        quantity = iim.Quantity;
                        break;
                    }
                }
                newItems.Add(new Item(im.Id, im.Name, quantity));
                quantity = 0;
            }

            itemInventoryList = newItems;
            itemInventoryList = newItems.OrderBy(o => o.Name).ToList();
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
            surfaceListBox2.DataContext = itemInventoryList;
        }

        private void SurfaceButton_Click(object sender, RoutedEventArgs e) // Hides/shows items
        {
            if (surfaceListBox2.Visibility != Visibility.Collapsed)
                surfaceListBox2.Visibility = Visibility.Collapsed;
            else
                surfaceListBox2.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Event for when item is selected.
        /// </summary>
        private void SurfaceListBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EventHandler handler = SurfaceListBox2SelectedValueChanged;
            if (handler != null)
                handler(sender, e);
        }

        private class Item
        {
            public Item(int i, string n, double q)
            {
                Id = i;
                Name = n;
                Quantity = q;
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public double Quantity { get; set; }
        }
    }
}