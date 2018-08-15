using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Market.xaml
    /// </summary>
    public partial class Market
    {
        //Storing variables
        public List<AssetInventoryModel> Assetlist;
        public List<GroupModel> Groups;
        public List<ItemInventoryModel> Itemlist;
        private AssetModel _asset;
        private ItemModel _item;
        private int _tempAssetPercent;
        private double _tempItemAmount;

        public Market()
        {
            InitializeComponent();
            Groups = new List<GroupModel>();
        }

        //Event Handlers
        public event EventHandler SendTrade;
        public event EventHandler TalkToBank;
        public event EventHandler BuyFromMarket;

        public void TrySetAssetDataContext()
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                SetAssetDataContext();
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(SetAssetDataContext), DispatcherPriority.Normal);
            }
        }

        private void SetAssetDataContext()
        {
            var listBoxItems = new List<GroupModel>();

            foreach (GroupModel g in Groups)
                foreach (AssetInventoryModel a in Assetlist)
                    if (a.GroupId == g.Id)
                        listBoxItems.Add(g);

            groupsListBox.DataContext = listBoxItems;
            if (listBoxItems.Count == 0 && ConditionalTradeGrid.Visibility == Visibility.Visible)
                noOwners.Visibility = Visibility.Visible;
            else
                noOwners.Visibility = Visibility.Collapsed;

            if (ShowAvalibleAssetPercent() == 0 && ConditionalTradeGrid.Visibility == Visibility.Collapsed)
                noAssets.Visibility = Visibility.Visible;
            else
                noAssets.Visibility = Visibility.Collapsed;
        }

        public void TrySetItemDataContext()
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                SetItemDataContext();
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(SetItemDataContext), DispatcherPriority.Normal);
            }
        }

        public void SetItemDataContext()
        {
            var listBoxItems = new List<GroupModel>();

            foreach (GroupModel g in Groups)
                foreach (ItemInventoryModel i in Itemlist)
                    if (i.GroupId == g.Id)
                        listBoxItems.Add(g);

            groupsListBox.DataContext = listBoxItems;
            groupsListBox.SelectedIndex = 1;
            if (listBoxItems.Count == 0 && ConditionalTradeGrid.Visibility == Visibility.Visible)
                noOwners.Visibility = Visibility.Visible;
            else
                noOwners.Visibility = Visibility.Collapsed;

            if (ConditionalTradeGrid.Visibility == Visibility.Visible)
                noAssets.Visibility = Visibility.Collapsed;
        }

        public void GetAssetInformation(AssetModel a, List<AssetInventoryModel> la)
        {
            _asset = a;
            Assetlist = la;
            _item = null;
            Itemlist = null;
            SetContent();
        }

        public void GetItemInformation(ItemModel i, List<ItemInventoryModel> li)
        {
            _item = i;
            Itemlist = li;
            _asset = null;
            Assetlist = null;
            SetContent();
        }

        private int ShowAvalibleAssetPercent()
        {
            int percent = 100;
            if (Assetlist != null)
                foreach (AssetInventoryModel e in Assetlist)
                    percent -= e.Share;
            return percent;
        }

        private void SetContent()
        {
            if (_item == null)
            {
                assetName.Text = _asset.Name;
                CTprice.Text = _asset.Price.ToString();
                OMprice.Text = String.Format("{0:C}", (_asset.Price/20));
                CTamount.Text = "0";
                OMamount.Text = ShowAvalibleAssetPercent().ToString();
                CTmessage.Text = "Type in a message...";
                OMpriceLabel.Content = "Asset price (5%) ";
                OMtotalLabel.Content = "Total ";
                CTpriceLabel.Content = "Total asset price $";
                CTamountLabel.Content = "Percent %";
                OMamountLabel.Content = "Percent %";
                CTtotalLabel.Content = "";
                CTtotal.Visibility = Visibility.Collapsed;

                Double amnt = Convert.ToDouble(OMamount.Text)/5;
                OMtotal.Text = String.Format("{0:C}", (_asset.Price/20)*amnt);

                CTbtnAmuntUp.Visibility = Visibility.Collapsed;
                btnAmuntUp.Visibility = Visibility.Collapsed;
                btnAmuntDown.Visibility = Visibility.Visible;
                CTbtnAmuntDown.Visibility = Visibility.Collapsed;
                CTtotal.Text = String.Format("{0:C}", _asset.Price*100);
                if (ShowAvalibleAssetPercent() <= 5)
                {
                    btnAmuntDown.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                assetName.Text = _item.Name;
                CTprice.Text = _item.Value.ToString();
                OMprice.Text = String.Format("{0:C}", _item.Value);
                CTamount.Text = "1";
                OMamount.Text = "1";
                CTmessage.Text = "Type in a message...";
                OMpriceLabel.Content = "Price each item";
                OMtotalLabel.Content = "Total ";
                CTamountLabel.Content = "Amount";
                OMamountLabel.Content = "Amount";
                CTpriceLabel.Content = "Price each item $";
                CTtotalLabel.Content = "Total ";
                CTtotal.Visibility = Visibility.Visible;
                OMtotal.Visibility = Visibility.Visible;
                CTbtnAmuntUp.Visibility = Visibility.Visible;
                btnAmuntUp.Visibility = Visibility.Visible;
                CTbtnAmuntDown.Visibility = Visibility.Collapsed;
                btnAmuntDown.Visibility = Visibility.Collapsed;
                CTtotal.Text = String.Format("{0:C}", _item.Value*1);
                OMtotal.Text = String.Format("{0:C}", _item.Value*1);
            }
        }

        private void ChangeTrade(object o, EventArgs e)
        {
            var b = (Button) o;
            if (b.Name == "omButton")
            {
                omButton.Background = new SolidColorBrush(Color.FromRgb(255, 123, 36));
                ctButton.Background = new SolidColorBrush(Colors.Gray);
                OpenMarketGrid.Visibility = Visibility.Visible;
                ConditionalTradeGrid.Visibility = Visibility.Collapsed;
                noOwners.Visibility = Visibility.Collapsed;

                if (ShowAvalibleAssetPercent() == 0)
                    noAssets.Visibility = Visibility.Visible;
                else
                    noAssets.Visibility = Visibility.Collapsed;
            }
            else
            {
                omButton.Background = new SolidColorBrush(Colors.Gray);
                ctButton.Background = new SolidColorBrush(Color.FromRgb(255, 123, 36));
                OpenMarketGrid.Visibility = Visibility.Collapsed;
                ConditionalTradeGrid.Visibility = Visibility.Visible;
                buyButton.Visibility = Visibility.Visible;
                bankWarning.Visibility = Visibility.Collapsed;

                if (!groupsListBox.HasItems)
                    noOwners.Visibility = Visibility.Visible;
                else
                    noOwners.Visibility = Visibility.Collapsed;

                noAssets.Visibility = Visibility.Collapsed;
            }
        }

        private void Buy_Market(object o, EventArgs e)
        {
            var dt = new DateTime();
            var newTrade = new TradingModel(0, 0, 0,"", 0, 0, 0.0, Convert.ToInt32(OMamount.Text), "", dt, 0, "", 0.0);
            if (_asset != null)
            {
                newTrade.AssetId = _asset.Id;
                newTrade.Price = (_asset.Price/20)*(Convert.ToInt32(OMamount.Text)/5);
                newTrade.PurcaseName = _asset.Name;
            }
            else
            {
                newTrade.ItemId = _item.Id;
                newTrade.Price = _item.Value*Convert.ToInt32(OMamount.Text);
                newTrade.PurcaseName = _item.Name;
            }

            EventHandler handler = TalkToBank;
            if (handler != null)
                handler(newTrade, e);
        }

        public void ConfirmWithBank(TradingModel newTrade, bool bank)
        {
            if (bank)
            {
                EventHandler handler = BuyFromMarket;

                if (handler != null)
                    handler(newTrade, new EventArgs());
            }
            else
            {
                bankWarning.Visibility = Visibility.Visible;
                buyButton.Visibility = Visibility.Collapsed;
            }
        }

        //Conditional Trading - Asking to buy an asset or item from a group
        private void Buy_Trading(object o, EventArgs e)
        {
            var dt = new DateTime();
            var newTrade = new TradingModel(0, 0, 0,"", 0, 0, double.Parse(CTprice.Text), Convert.ToInt32(CTamount.Text),
                                            "", dt, 0, "", 0);

            if (CTmessage.Text == "Type in a message...")
                newTrade.Text = "";
            else
                newTrade.Text = CTmessage.Text;

            if (groupsListBox.SelectedIndex == -1)
            {
                //buyButton.Content = "Choose a group!";
                return;
            }

            var selected = (GroupModel) groupsListBox.SelectedItem;
            newTrade.Owner = selected.Id;

            if (_asset != null)
            {
                newTrade.AssetId = _asset.Id;
                newTrade.ItemId = 0;
                newTrade.PurcaseName = _asset.Name;
            }
            else
            {
                newTrade.AssetId = 0;
                newTrade.ItemId = _item.Id;
                newTrade.PurcaseName = _item.Name;
                newTrade.TotalPrice = newTrade.Price*newTrade.Amount;
            }
            groupsListBox.SelectedIndex = -1;

            EventHandler handler = SendTrade;
            if (handler != null)
                handler(newTrade, e);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) sender;
            tb.Text = string.Empty;
            tb.GotFocus -= TextBox_GotFocus;
        }

        private void PriceInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var regex = new Regex(@"^\d$");
            String check = CTprice.Text;

            if (!regex.IsMatch(check) && CTprice.Text != "")
                CTprice.Text = new String(check.Where(Char.IsDigit).ToArray());

            if (CTprice.Text != "" && CTamount.Text != "")
            {
                double tot = double.Parse(CTprice.Text)*Convert.ToInt32(CTamount.Text);
                CTtotal.Text = "$" + Convert.ToString(tot);
            }
            else
                CTtotal.Text = "$ 0";
        }

        private void AmountUp_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button) sender;
            if (b.Name == "CTbtnAmuntUp")
            {
                if (_asset != null)
                {
                    int am = _tempAssetPercent;
                    int getam = Convert.ToInt32(CTamount.Text);
                    CTamount.Text = Convert.ToString(getam + 5);
                    if (am != null)
                    {
                        if (Convert.ToInt32(CTamount.Text) == am)
                            CTbtnAmuntUp.Visibility = Visibility.Collapsed;
                        else
                            CTbtnAmuntUp.Visibility = Visibility.Visible;

                        CTbtnAmuntDown.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    int getam = Convert.ToInt32(CTamount.Text);
                    CTamount.Text = Convert.ToString(getam + 1);

                    if (Convert.ToInt32(CTamount.Text) == _tempItemAmount)
                        CTbtnAmuntUp.Visibility = Visibility.Collapsed;
                    else
                        CTbtnAmuntUp.Visibility = Visibility.Visible;
                    CTbtnAmuntDown.Visibility = Visibility.Visible;
                }

                CTtotal.Text = String.Format("{0:C}", double.Parse(CTprice.Text)*Convert.ToInt32(CTamount.Text));
            }
            else
            {
                if (_asset != null)
                {
                    int am = ShowAvalibleAssetPercent();
                    int getam = Convert.ToInt32(OMamount.Text);
                    OMamount.Text = Convert.ToString(getam + 5);

                    if (Convert.ToInt32(OMamount.Text) == am)
                        btnAmuntUp.Visibility = Visibility.Collapsed;
                    else
                    {
                        btnAmuntDown.Visibility = Visibility.Visible;
                        btnAmuntUp.Visibility = Visibility.Visible;
                    }
                    int divide = Convert.ToInt32(OMamount.Text)/5;
                    OMtotal.Text = String.Format("{0:C}", (_asset.Price/20)*divide);
                }
                else
                {
                    int getam = Convert.ToInt32(OMamount.Text);
                    OMamount.Text = Convert.ToString(getam + 1);

                    if (Convert.ToInt32(CTamount.Text) == 1000)
                        btnAmuntUp.Visibility = Visibility.Collapsed;
                    else
                    {
                        btnAmuntDown.Visibility = Visibility.Visible;
                        btnAmuntUp.Visibility = Visibility.Visible;
                    }
                    OMtotal.Text = String.Format("{0:C}", _item.Value*Convert.ToInt32(OMamount.Text));
                }
            }
        }

        private void AmountDown_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button) sender;
            if (b.Name == "CTbtnAmuntDown")
            {
                if (_asset != null)
                {
                    int getam = Convert.ToInt32(CTamount.Text);
                    CTamount.Text = Convert.ToString(getam - 5);

                    if (Convert.ToInt32(CTamount.Text) <= 5)
                        CTbtnAmuntDown.Visibility = Visibility.Collapsed;

                    else
                        CTbtnAmuntDown.Visibility = Visibility.Visible;

                    CTbtnAmuntUp.Visibility = Visibility.Visible;
                }

                else
                {
                    int getam = Convert.ToInt32(CTamount.Text);
                    CTamount.Text = Convert.ToString(getam - 1);
                    if (Convert.ToInt32(CTamount.Text) <= 1)
                        CTbtnAmuntDown.Visibility = Visibility.Collapsed;
                    else
                        CTbtnAmuntDown.Visibility = Visibility.Visible;

                    CTbtnAmuntUp.Visibility = Visibility.Visible;
                }
                CTtotal.Text = String.Format("{0:C}", double.Parse(CTprice.Text)*Convert.ToInt32(CTamount.Text));
            }

            else
            {
                if (_asset != null)
                {
                    int getam = Convert.ToInt32(OMamount.Text);
                    OMamount.Text = Convert.ToString(getam - 5);

                    if (Convert.ToInt32(OMamount.Text) <= 5)
                        btnAmuntDown.Visibility = Visibility.Collapsed;
                    else
                    {
                        btnAmuntDown.Visibility = Visibility.Visible;
                        btnAmuntUp.Visibility = Visibility.Visible;
                    }
                    if (bankWarning.Visibility == Visibility.Visible)
                    {
                        buyButton.Visibility = Visibility.Visible;
                        bankWarning.Visibility = Visibility.Collapsed;
                    }
                    int divide = Convert.ToInt32(OMamount.Text)/5;
                    OMtotal.Text = String.Format("{0:C}", (_asset.Price/20)*divide);
                }
                else
                {
                    int getam = Convert.ToInt32(OMamount.Text);
                    OMamount.Text = Convert.ToString(getam - 1);
                    if (Convert.ToInt32(OMamount.Text) <= 1)
                        btnAmuntDown.Visibility = Visibility.Collapsed;
                    else
                    {
                        btnAmuntDown.Visibility = Visibility.Visible;
                        btnAmuntUp.Visibility = Visibility.Visible;
                    }
                    if (bankWarning.Visibility == Visibility.Visible)
                    {
                        buyButton.Visibility = Visibility.Visible;
                        bankWarning.Visibility = Visibility.Collapsed;
                    }
                    OMtotal.Text = String.Format("{0:C}", _item.Value*Convert.ToInt32(OMamount.Text));
                }
            }
        }

        private void NewSelectedGroup(object sender, RoutedEventArgs e)
        {
            var group = (GroupModel) groupsListBox.SelectedValue;
            if (group != null)
            {
                if (Itemlist == null)
                {
                    var a = new AssetInventoryModel(0, 0, 0, 0);
                    foreach (AssetInventoryModel k in Assetlist)
                        if (group.Id == k.GroupId)
                            a = k;
                    _tempAssetPercent = a.Share;
                    CTamount.Text = a.Share.ToString();
                    CTbtnAmuntUp.Visibility = Visibility.Collapsed;

                    if (a.Share <= 5)
                        CTbtnAmuntDown.Visibility = Visibility.Collapsed;
                    else
                        CTbtnAmuntDown.Visibility = Visibility.Visible;
                }
                else
                {
                    var i = new ItemInventoryModel(0, 0, 0, 0);
                    foreach (ItemInventoryModel k in Itemlist)
                        if (group.Id == k.GroupId)
                            i = k;
                    _tempItemAmount = i.Quantity;
                    CTamount.Text = i.Quantity.ToString();
                    CTbtnAmuntUp.Visibility = Visibility.Collapsed;

                    if (i.Quantity <= 1)
                        CTbtnAmuntDown.Visibility = Visibility.Collapsed;
                    else
                        CTbtnAmuntDown.Visibility = Visibility.Visible;
                }
            }
        }
    }
}