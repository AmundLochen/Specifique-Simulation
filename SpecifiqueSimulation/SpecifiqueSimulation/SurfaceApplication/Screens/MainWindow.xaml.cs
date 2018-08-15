using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClientLibrary;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Generic;
using Model;

namespace SurfaceApplication.Screens
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int Port = 8888;
        private readonly Clnt _client;
        private readonly List<String> _views;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            string ip = Oclass.GetIpAddress();

            //  Starts client
            _client = new Clnt(ip, Port);
            _views = new List<String>();

            //  Gameboard
            gameboard.GameboardSelectedValueChanged += GameboardAsset_TouchDown;

            //  Inventory
            inventory.assetInventory.SurfaceListBox1SelectedValueChanged +=
                AssetInventory_surfaceListBox1SelectedValueChanged;
            inventory.itemInventory.SurfaceListBox2SelectedValueChanged +=
                ItemInventory_surfaceListBox2SelectedValueChanged;

            //  Asset
            assetWindow.TradeButtonPressed += AssetWindow_tradeButtonPressed;

            //  Item
            itemWindow.TradeButtonPressed += ItemWindow_tradeButtonPressed;

            //  Inbox
            inbox.ShowMessageHistory += ShowMessages;
            inbox.ShowMessageNotification += TryUpdateMessageIcon;
            inbox.TradeAnswered += MessageHistory_tradeAnswered;

            //  Message History
            messageHistory.SendMessageClicked += MessageHistory_sendMessageClicked;

            //  Send message
            viewSend.SendButtonClicked += ViewSend_sendButtonClicked;

            //  Market
            viewMarket.SendTrade += SendNewTrade;
            viewMarket.BuyFromMarket += BuyFromMarket;
            viewMarket.TalkToBank += CheckWithBank;

            //  Notification
            viewNot.ShowNotificationNotification += TryUpdateNotificationIcon;

            //MoneyStream
            viewStream.ExpandMenu += ExpandMenuBar;

            //  Client
            _client.RecievedObject += Client_recievedObject;

            //  Start
            startView.GroupsSelectedValueChanged += Start_groupsSelectedValueChanged;

            //  Menubar settings
            menuBar.ApplyTemplate();
            menuBar.Background = new SolidColorBrush(Colors.Transparent);
            menuBar.ShowsActivationEffects = false;
            var ssc = menuBar.Template.FindName("shadow", menuBar) as SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }

        private void TryUpdateNotificationIcon(object sender, EventArgs eventArgs)
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                UpdateNotificationIcon();
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(UpdateNotificationIcon), DispatcherPriority.Normal);
            }
        }

        private void TryUpdateMessageIcon(object sender, EventArgs eventArgs)
        {
            try
            {
                // Check if this thread has access to this object.
                Dispatcher.VerifyAccess();

                // The thread has access to the object, so update the UI.
                UpdateMessageIcon();
            }

                // Cannot access objects on the thread. 
            catch (InvalidOperationException)
            {
                // Placing job onto the Dispatcher of the UI Thread.
                Dispatcher.BeginInvoke(new Action(UpdateMessageIcon), DispatcherPriority.Normal);
            }
        }

        private void UpdateMessageIcon()
        {
            (showInbox.Content as Image).Source = new BitmapImage(new Uri("/Images/InboxNote.png", UriKind.Relative));
        }

        private void UpdateNotificationIcon()
        {
            (showNotif.Content as Image).Source =
                new BitmapImage(new Uri("/Images/NotificationNoti.png", UriKind.Relative));
        }

        private void ShowMessages(object sender, EventArgs eventArgs)
        {
            List<MessageModel> messages = inbox.GetMessages;
            // List<MessageModel> conversation = data.getConversation(inbox.selectedMessage.parentId);
            messageHistory.ShowMessageHistory(messages);
            AddView("sviHistoryWindow");
            sviHistoryWindow.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Shows asset window when selected from gameboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void GameboardAsset_TouchDown(object sender, EventArgs eventArgs)
        {
            // Gets AssetModel and sets asset window
            int id = gameboard.Id;
            AssetModel am = inventory.assetInventory.GetAssetModel(id);
            assetWindow.SetAsset(am, inventory.Groups, inventory.assetInventory.GetAssetInventoryModels(id));

            if (sviAssetWindow.Visibility != Visibility.Visible)
                SetAssetWindowVisible();

            inventory.assetInventory.SetSurfaceListBox1SelectedValue(id);
        }

        /// <summary>
        ///     Shows asset window when selected in inventory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AssetInventory_surfaceListBox1SelectedValueChanged(object sender, EventArgs eventArgs)
        {
            // Gets ItemModel and sets item window
            if (inventory.assetInventory.SurfaceListBox1.SelectedIndex == -1)
                return;
            int id = inventory.assetInventory.AssetId;
            AssetModel am = inventory.assetInventory.GetAssetModel(id);

            assetWindow.SetAsset(am, inventory.Groups, inventory.assetInventory.GetAssetInventoryModels(id));

            // Positions asset window and makes it visible
            gameboard.SetGameboardAsset(am.Id);
            //sviAssetWindow.Center = new Point(gameboard.marginLeft, gameboard.marginTop);
            if (sviAssetWindow.Visibility != Visibility.Visible)
                SetAssetWindowVisible();

            if (sviMarketWindow.Visibility == Visibility.Visible)
            {
                List<AssetInventoryModel> getAsset = inventory.assetInventory.GetAssetInventoryModels(am.Id);
                viewMarket.Assetlist = getAsset;
                viewMarket.TrySetAssetDataContext();
                viewMarket.GetAssetInformation(am, getAsset);
            }
        }

        /// <summary>
        ///     Called when recieving an object from Server.
        /// </summary>
        /// <param name="recieved"></param>
        /// <param name="eventArgs"></param>
        private void Client_recievedObject(object recieved, EventArgs eventArgs)
        {
            Type recievedType = recieved.GetType();

            //  Object is group number.
            if (recievedType == typeof (DateTime))
            {
                var recieve = (DateTime) recieved;

                clock.SetTime(recieve);
                clock.TryShowTime();

                //  Checks if any trades has expired.
                foreach (TradingModel t in inbox.CheckForExpiredTrades(recieve))
                {
                    _client.SendTrading(t);
                }
            }

            else if (recievedType == typeof (bool))
            {
                var paused = (bool) recieved;

                if (paused)
                {
                    Dispatcher.Invoke((Action) (() => { pauseView.Visibility = Visibility.Visible; }));
                }

                else
                {
                    Dispatcher.Invoke((Action) (() =>
                        {
                            pauseView.Visibility = Visibility.Hidden;
                            startView.Visibility = Visibility.Hidden;
                        }));
                }
            }

            else if (recievedType == typeof (List<CashPerTeamModel>))
            {
                var s = (List<CashPerTeamModel>) recieved;
                for (int i = 0; i < s.Count; i++)
                {
                    if (i + 1 == inventory.GroupNumber)
                    {
                        //Debug.WriteLine("This is the cash: " + s[i].cash);
                        viewStream.UpdateValues(s[i].Cash.ToString());
                    }
                }

                viewValues.UpdateGroups(s);
                //Put where to send cash array here.
            }

            else if (recievedType == typeof (int))
            {
                var groupNumber = (int) recieved;

                if (groupNumber != 0)
                {
                    foreach (var e in inventory.Groups)
                        if ((int)recieved == e.Id)
                            viewStream.Group = e.Name;

                    viewStream.SetGroup();
                    inventory.GroupNumber = (int) recieved;
                    inbox.YourGroup = inventory.GroupNumber;
                    inventory.SetGroups(inventory.Groups);
                    viewSend.Groups = inventory.OtherGroups;
                    viewSend.TrySetDataContext();
                    viewMarket.Groups = inventory.OtherGroups;
                }

                else
                {
                    startView.SetInfoLabel();
                }
            }

            else if (recievedType == typeof (MessageModel))
            {
                var message = (MessageModel) recieved;

                inbox.AddInboxMessage(message);

                if (sviHistoryWindow.Visibility == Visibility.Visible)
                    messageHistory.UpdateConversation(message);
            }

            else if (recievedType == typeof (NotificationModel))
            {
                var notification = (NotificationModel) recieved;
                viewNot.AddNotification(notification);
            }

            else if (recievedType == typeof (TradingModel))
            {
                var trading = (TradingModel) recieved;
                //trading.text = trading.time.ToString();
                //Console.WriteLine("Recieved trade: " + trading.text);
                inbox.AddInboxTrade(trading);
            }

            else if (recievedType == typeof (AssetInventoryModel))
            {
                var assetInventory = (AssetInventoryModel) recieved;

                // Finds appropriate action to take with the AssetInventory.
                if (inventory.assetInventory.AssetInventoryExists(assetInventory))
                {
                    inventory.assetInventory.EditAssetInventoryModel(assetInventory);
                }
                else
                {
                    inventory.assetInventory.AddAssetInventoryModel(assetInventory);
                }

                //  Updates inventory.
                inventory.assetInventory.AddAssets(inventory.assetInventory.Assets,
                                                   inventory.assetInventory.GetGroupsAssetInventoryModels(
                                                       inventory.GroupNumber));

                // Updates asset window.
                if (sviAssetWindow.Visibility == Visibility.Visible)
                    assetWindow.TryToUpdate(assetWindow.Assets, inventory.Groups,
                                            inventory.assetInventory.GetAssetInventoryModels(assetWindow.Assets.Id));
            }

            else if (recievedType == typeof (ItemInventoryModel))
            {
                var itemInventory = (ItemInventoryModel) recieved;

                // Finds appropriate action to take with the ItemInventory.
                if (inventory.itemInventory.ItemInventoryExists(itemInventory))
                {
                    inventory.itemInventory.EditItemInventoryModel(itemInventory);
                }
                else
                {
                    inventory.itemInventory.AddItemInventoryModel(itemInventory);
                }

                //  Updates inventory.
                inventory.itemInventory.AddItems(inventory.itemInventory.Items,
                                                 inventory.itemInventory.GetGroupsItemInventoryModels(
                                                     inventory.GroupNumber));

                // Updates item window.
                if (sviItemWindow.Visibility == Visibility.Visible)
                    itemWindow.TryToUpdate(itemWindow.ItemModel, inventory.Groups,
                                           inventory.itemInventory.GetItemInventoryModels(itemWindow.ItemModel.Id));
            }

            else if (recievedType == typeof (ItemModel))
            {
                var item = (ItemModel) recieved;

                inventory.itemInventory.EditItemPrice(item);

                //  Updates inventory.
                inventory.itemInventory.AddItems(inventory.itemInventory.Items,
                                                 inventory.itemInventory.GetGroupsItemInventoryModels(
                                                     inventory.GroupNumber));

                // Updates item window.
                if (sviItemWindow.Visibility == Visibility.Visible && itemWindow.ItemModel.Id == item.Id)
                    itemWindow.TryToUpdate(item, inventory.Groups,
                                           inventory.itemInventory.GetItemInventoryModels(itemWindow.ItemModel.Id));
            }

            else if (recievedType == typeof (List<GroupModel>))
            {
                var groups = (List<GroupModel>) recieved;

                startView.SetGroups(groups);
                inventory.SetGroups(groups);
                viewValues.Groups = inventory.Groups;
            }

            else if (recievedType == typeof (List<AssetModel>))
            {
                var assets = (List<AssetModel>) recieved;

                inventory.assetInventory.Assets = assets;

                //  Updates inventory.
                inventory.assetInventory.AddAssets(inventory.assetInventory.Assets,
                                                   inventory.assetInventory.AssetInventories);
            }

            else if (recievedType == typeof (List<AssetInventoryModel>))
            {
                var assetInventories = (List<AssetInventoryModel>) recieved;

                inventory.assetInventory.AssetInventories = assetInventories;

                //  Updates inventory.
                inventory.assetInventory.AddAssets(inventory.assetInventory.Assets,
                                                   inventory.assetInventory.GetGroupsAssetInventoryModels(
                                                       inventory.GroupNumber));
            }

            else if (recievedType == typeof (List<ItemModel>))
            {
                var items = (List<ItemModel>) recieved;

                inventory.itemInventory.Items = items;

                //  Updates inventory.
                inventory.itemInventory.AddItems(inventory.itemInventory.Items, inventory.itemInventory.ItemInventories);
            }

            else if (recievedType == typeof (List<ItemInventoryModel>))
            {
                var itemInventories = (List<ItemInventoryModel>) recieved;

                inventory.itemInventory.ItemInventories = itemInventories;

                //  Updates inventory.
                inventory.itemInventory.AddItems(inventory.itemInventory.Items,
                                                 inventory.itemInventory.GetGroupsItemInventoryModels(
                                                     inventory.GroupNumber));
            }

            else if (recievedType == typeof (List<MessageModel>))
            {
                var messages = (List<MessageModel>) recieved;

                foreach (MessageModel m in messages)
                {
                    inbox.AddInboxMessage(m);
                }
            }

            else if (recievedType == typeof (List<NotificationModel>))
            {
                var notifications = (List<NotificationModel>) recieved;

                foreach (NotificationModel n in notifications)
                {
                    viewNot.AddNotification(n);
                }
            }

            else if (recievedType == typeof(List<TradingModel>))
            {
                var trades = (List<TradingModel>)recieved;

                foreach (TradingModel t in trades)
                {
                    inbox.AddInboxTrade(t);
                }
            }
        }

        /// <summary>
        ///     Shows item window when selected in inventory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ItemInventory_surfaceListBox2SelectedValueChanged(object sender, EventArgs eventArgs)
        {
            // Gets ItemModel and sets item window
            if (inventory.itemInventory.surfaceListBox2.SelectedIndex == -1)
                return;
            int id = inventory.itemInventory.ItemId;
            ItemModel im = inventory.itemInventory.GetItemModel(id);


            // Positions item window and makes it visible
            itemWindow.SetItem(im, inventory.Groups, inventory.itemInventory.GetItemInventoryModels(im.Id));
            if (sviItemWindow.Visibility != Visibility.Visible)
                SetItemWindowVisible();

            if (sviMarketWindow.Visibility == Visibility.Visible)
            {
                List<ItemInventoryModel> getItem = inventory.itemInventory.GetItemInventoryModels(im.Id);
                viewMarket.Itemlist = inventory.itemInventory.GetItemInventoryModels(im.Id);
                viewMarket.TrySetItemDataContext();
                viewMarket.GetItemInformation(im, getItem);
            }
        }

        /// <summary>
        ///     Changes asset window's position and orientation when it's been manipulated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SviAssetWindow_ContainerManipulationCompleted(object sender,
                                                                   ContainerManipulationCompletedEventArgs e)
        {
            double newOrientation = 0;
            double orientation = sviAssetWindow.ActualOrientation;

            if (orientation > 90 && orientation < 270)
                newOrientation = 180;

            else if (orientation > 180)
                newOrientation = 360;

            var d = new Duration(TimeSpan.FromMilliseconds(300));
            var da = new DoubleAnimation(orientation, newOrientation, d);
            var sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(da, sviAssetWindow);
            Storyboard.SetTargetProperty(da, new PropertyPath("Orientation"));
            sb.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            sviAssetWindow.Orientation = newOrientation;

            double x = sviAssetWindow.Center.X;
            double y = sviAssetWindow.Center.Y;

            var point = new Point(sviAssetWindow.Center.X, sviAssetWindow.Center.Y);

            if (x < 260)
                point.X = 260;

            if (x > 1660)
                point.X = 1660;

            if (y < 160)
                point.Y = 160;

            if (y > 920)
                point.Y = 920;

            bool remove = false;
            if (y < 140 && x < 140)
            {
                point.Y = 300;
                point.X = 300;
                remove = true;
            }
            if (remove)
                AssetWindow_exitButtonPressed();

            var da2 = new PointAnimation(sviAssetWindow.ActualCenter, point, d);

            var sb2 = new Storyboard();
            sb2.Children.Add(da2);

            Storyboard.SetTarget(da2, sviAssetWindow);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Center"));
            sb2.FillBehavior = FillBehavior.Stop;

            sb2.Begin();
            sviAssetWindow.Center = point;
        }

        private void SviHistoryWindow_ContainerManipulationCompleted(object sender,
                                                                     ContainerManipulationCompletedEventArgs e)
        {
            double newOrientation = 0;
            double orientation = sviHistoryWindow.ActualOrientation;

            if (orientation > 90 && orientation < 270)
                newOrientation = 180;

            else if (orientation > 180)
                newOrientation = 360;

            var d = new Duration(TimeSpan.FromMilliseconds(300));
            var da = new DoubleAnimation(orientation, newOrientation, d);
            var sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(da, sviHistoryWindow);
            Storyboard.SetTargetProperty(da, new PropertyPath("Orientation"));
            sb.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            sviHistoryWindow.Orientation = newOrientation;

            double x = sviHistoryWindow.Center.X;
            double y = sviHistoryWindow.Center.Y;

            var point = new Point(sviHistoryWindow.Center.X, sviHistoryWindow.Center.Y);

            if (x < 230)
                point.X = 230;

            if (x > 1700)
                point.X = 1700;

            if (y < 255)
                point.Y = 255;

            if (y > 870)
                point.Y = 870;

            bool remove = false;
            if (y < 140 && x < 140)
            {
                point.Y = 350;
                point.X = 350;
                remove = true;
            }
            if (remove)
                sviHistoryWindow.Visibility = Visibility.Collapsed;

            var da2 = new PointAnimation(sviHistoryWindow.ActualCenter, point, d);

            var sb2 = new Storyboard();
            sb2.Children.Add(da2);

            Storyboard.SetTarget(da2, sviHistoryWindow);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Center"));
            sb2.FillBehavior = FillBehavior.Stop;

            sb2.Begin();
            sviHistoryWindow.Center = point;
        }

        private void SviMarketWindow_ContainerManipulationCompleted(object sender,
                                                                    ContainerManipulationCompletedEventArgs e)
        {
            double newOrientation = 0;
            double orientation = sviMarketWindow.ActualOrientation;

            if (orientation > 90 && orientation < 270)
                newOrientation = 180;

            else if (orientation > 180)
                newOrientation = 360;

            var d = new Duration(TimeSpan.FromMilliseconds(300));
            var da = new DoubleAnimation(orientation, newOrientation, d);
            var sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(da, sviMarketWindow);
            Storyboard.SetTargetProperty(da, new PropertyPath("Orientation"));
            sb.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            sviMarketWindow.Orientation = newOrientation;

            double x = sviMarketWindow.Center.X;
            double y = sviMarketWindow.Center.Y;

            var point = new Point(sviMarketWindow.Center.X, sviMarketWindow.Center.Y);

            if (x < 195)
                point.X = 195;

            if (x > 1700)
                point.X = 1700;

            if (y < 280)
                point.Y = 280;

            if (y > 850)
                point.Y = 850;

            bool remove = false;
            if (y < 140 && x < 140)
            {
                point.Y = 300;
                point.X = 400;
                remove = true;
            }
            if (remove)
                sviMarketWindow.Visibility = Visibility.Collapsed;

            var da2 = new PointAnimation(sviMarketWindow.ActualCenter, point, d);

            var sb2 = new Storyboard();
            sb2.Children.Add(da2);

            Storyboard.SetTarget(da2, sviMarketWindow);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Center"));
            sb2.FillBehavior = FillBehavior.Stop;

            sb2.Begin();
            sviMarketWindow.Center = point;
        }


        /// <summary>
        ///     Sets asset window visible and animates it.
        /// </summary>
        private void SetAssetWindowVisible()
        {
            var startingPosition = new Point(960, 160);
            var endingPosition = new Point(960, 500);
            var d = new Duration(TimeSpan.FromMilliseconds(500));

            sviAssetWindow.Center = startingPosition;

            sviAssetWindow.Visibility = Visibility.Visible;

            var ani = new PointAnimation(startingPosition, endingPosition, d);

            var sb = new Storyboard();
            sb.Children.Add(ani);

            Storyboard.SetTarget(ani, sviAssetWindow);
            Storyboard.SetTargetProperty(ani, new PropertyPath("Center"));
            ani.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            sviAssetWindow.Center = endingPosition;
            AddView("sviAssetWindow");
        }

        /// <summary>
        ///     Sets item window visible and animates it.
        /// </summary>
        private void SetItemWindowVisible()
        {
            var startingPosition = new Point(960, 160);
            var endingPosition = new Point(960, 500);
            var d = new Duration(TimeSpan.FromMilliseconds(500));

            sviItemWindow.Center = startingPosition;

            sviItemWindow.Visibility = Visibility.Visible;

            var ani = new PointAnimation(startingPosition, endingPosition, d);

            var sb = new Storyboard();
            sb.Children.Add(ani);

            Storyboard.SetTarget(ani, sviItemWindow);
            Storyboard.SetTargetProperty(ani, new PropertyPath("Center"));
            ani.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            sviItemWindow.Center = endingPosition;
            AddView("sviItemWindow");
        }

        /// <summary>
        ///     Makes menu bar drop down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuBarWindow_ContainerManipulationCompleted(object sender,
                                                                  ContainerManipulationCompletedEventArgs e)
        {
            const double newOrientation = 0;
            double orientation = menuBar.ActualOrientation;

            var d = new Duration(TimeSpan.FromMilliseconds(300));
            var da = new DoubleAnimation(orientation, newOrientation, d);
            var sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(da, menuBar);
            Storyboard.SetTargetProperty(da, new PropertyPath("Orientation"));
            sb.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            menuBar.Orientation = newOrientation;

            double y = menuBar.Center.Y;

            var point = new Point(menuBar.Center.X, menuBar.Center.Y);
            point.X = 360;
            if (y < 280)
                point.Y = 260;

            else
                point.Y = 530;

            var da2 = new PointAnimation(menuBar.ActualCenter, point, d);

            var sb2 = new Storyboard();
            sb2.Children.Add(da2);

            Storyboard.SetTarget(da2, menuBar);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Center"));
            sb2.FillBehavior = FillBehavior.Stop;

            sb2.Begin();
            menuBar.Center = point;
        }


        private void UpdateMenuBar()
        {
            const double newOrientation = 0;
            double orientation = menuBar.ActualOrientation;

            var d = new Duration(TimeSpan.FromMilliseconds(300));
            var da = new DoubleAnimation(orientation, newOrientation, d);
            var sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(da, menuBar);
            Storyboard.SetTargetProperty(da, new PropertyPath("Orientation"));
            sb.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            menuBar.Orientation = newOrientation;

            var point = new Point(menuBar.Center.X, menuBar.Center.Y);
            point.X = 360;
            point.Y = 530;

            var da2 = new PointAnimation(menuBar.ActualCenter, point, d);

            var sb2 = new Storyboard();
            sb2.Children.Add(da2);

            Storyboard.SetTarget(da2, menuBar);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Center"));
            sb2.FillBehavior = FillBehavior.Stop;

            sb2.Begin();
            menuBar.Center = point;
        }

        /// <summary>
        ///     Changes item window's position and orientation when it's been manipulated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SviItemWindow_ContainerManipulationCompleted(object sender,
                                                                  ContainerManipulationCompletedEventArgs e)
        {
            double newOrientation = 0;
            double orientation = sviItemWindow.ActualOrientation;

            if (orientation > 90 && orientation < 270)
                newOrientation = 180;

            else if (orientation > 180)
                newOrientation = 360;

            var d = new Duration(TimeSpan.FromMilliseconds(300));
            var da = new DoubleAnimation(orientation, newOrientation, d);
            var sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(da, sviItemWindow);
            Storyboard.SetTargetProperty(da, new PropertyPath("Orientation"));
            sb.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            sviItemWindow.Orientation = newOrientation;

            double x = sviItemWindow.Center.X;
            double y = sviItemWindow.Center.Y;

            var point = new Point(sviItemWindow.Center.X, sviItemWindow.Center.Y);

            if (x < 260)
                point.X = 260;

            if (x > 1660)
                point.X = 1660;

            if (y < 160)
                point.Y = 160;

            if (y > 920)
                point.Y = 920;

            bool remove = false;
            if (y < 140 && x < 140)
            {
                point.Y = 300;
                point.X = 300;
                remove = true;
            }
            if (remove)
                ItemWindow_exitButtonPressed();

            var da2 = new PointAnimation(sviItemWindow.ActualCenter, point, d);

            var sb2 = new Storyboard();
            sb2.Children.Add(da2);

            Storyboard.SetTarget(da2, sviItemWindow);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Center"));
            sb2.FillBehavior = FillBehavior.Stop;

            sb2.Begin();
            sviItemWindow.Center = point;
        }

        private void AssetWindow_exitButtonPressed()
        {
            sviAssetWindow.Visibility = Visibility.Hidden;
            inventory.assetInventory.SurfaceListBox1.SelectedIndex = -1;
        }

        private void ItemWindow_exitButtonPressed()
        {
            sviItemWindow.Visibility = Visibility.Hidden;
            inventory.itemInventory.surfaceListBox2.SelectedIndex = -1;
        }

        /// <summary>
        ///     Shows trade window for item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ItemWindow_tradeButtonPressed(object sender, EventArgs eventArgs)
        {
            sviMarketWindow.Visibility = Visibility.Visible;
            AddView("sviMarketWindow");
            ItemModel ite = itemWindow.ItemModel;
            List<ItemInventoryModel> getItem = inventory.itemInventory.GetItemInventoryModels(ite.Id);
            viewMarket.Itemlist = inventory.itemInventory.GetItemInventoryModels(ite.Id);
            viewMarket.TrySetItemDataContext();

            viewMarket.GetItemInformation(ite, getItem);
        }

        /// <summary>
        ///     Shows trade window for asset.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AssetWindow_tradeButtonPressed(object sender, EventArgs eventArgs)
        {
            sviMarketWindow.Visibility = Visibility.Visible;
            AddView("sviMarketWindow");
            /*List<AssetInventoryModel> getAsset = data.getAssetInventoryModels(;
            List<AssetInventoryModel> correctAsset = new List<AssetInventoryModel>();*/
            AssetModel ass = assetWindow.Assets;
            List<AssetInventoryModel> getAsset = inventory.assetInventory.GetAssetInventoryModels(ass.Id);
            viewMarket.Assetlist = getAsset;
            viewMarket.TrySetAssetDataContext();
            /*foreach(var e in getAsset)
                if(e.assetId == ass.id)
                    correctAsset.Add(e);*/

            viewMarket.GetAssetInformation(ass, getAsset);
        }

        private void ShowMes_Click(object sender, EventArgs eventArgs)
        {
            var b = (Button) sender;
            if (b.Name.Equals("showNotif"))
            {
                if (viewNot.Visibility == Visibility.Visible)
                {
                    viewNot.Visibility = Visibility.Collapsed;
                }
                else
                {
                    viewNot.Visibility = Visibility.Visible;
                    (showNotif.Content as Image).Source =
                        new BitmapImage(new Uri("/Images/Notification.png", UriKind.Relative));
                }
                inbox.Visibility = Visibility.Collapsed;
                viewSend.Visibility = Visibility.Collapsed;
            }
            else if (b.Name.Equals("showInbox"))
            {
                if (inbox.Visibility == Visibility.Visible)
                {
                    inbox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    inbox.Visibility = Visibility.Visible;
                    (showInbox.Content as Image).Source = new BitmapImage(new Uri("/Images/Inbox.png", UriKind.Relative));
                }

                viewSend.Visibility = Visibility.Collapsed;
                viewNot.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (viewSend.Visibility == Visibility.Visible)
                {
                    viewSend.Visibility = Visibility.Collapsed;
                }
                else
                {
                    viewSend.Visibility = Visibility.Visible;
                }

                inbox.Visibility = Visibility.Collapsed;
                viewNot.Visibility = Visibility.Collapsed;
            }
            UpdateMenuBar();
        }

        /// <summary>
        ///     Sends new message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewSend_sendButtonClicked(object sender, EventArgs e)
        {
            MessageModel outgoing = viewSend.Outgoing;
            outgoing.FromName = viewStream.Group;
            outgoing.From = inventory.GroupNumber;
            foreach(var v in viewValues.Groups)
                if(outgoing.To == v.Id)
                    outgoing.ToName = v.Name;

            _client.SendMessage(outgoing);
        }

        /// <summary>
        ///     Answers message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageHistory_sendMessageClicked(object sender, EventArgs e)
        {
            MessageModel outgoing = messageHistory.Outgoing;
            outgoing.From = inventory.GroupNumber;
            outgoing.FromName = viewStream.Group;
            _client.SendMessage(outgoing);
        }

        /// <summary>
        ///     Answers trade.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageHistory_tradeAnswered(object sender, EventArgs e)
        {
            var outgoing = (TradingModel) sender;
            _client.SendTrading(outgoing);
            //inbox.addInboxTrade(outgoing);
        }

        /// <summary>
        ///     Sends new trade.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendNewTrade(object sender, EventArgs e)
        {
            sviMarketWindow.Visibility = Visibility.Hidden;
            var newTrade = (TradingModel) sender;
            newTrade.Buyer = inventory.GroupNumber;
            newTrade.BuyerName = viewStream.Group;
            //inbox.addInboxTrade(newTrade);
            //inbox.TrySetDataContext();
            _client.SendTrading(newTrade);
        }

        private void BuyFromMarket(object sender, EventArgs e)
        {
            sviMarketWindow.Visibility = Visibility.Hidden;
            var newTrade = (TradingModel) sender;
            newTrade.Buyer = inventory.GroupNumber;
            newTrade.Accept = 2;
            _client.SendTrading(newTrade);
        }

        private void CheckWithBank(object sender, EventArgs e)
        {
            var tm = (TradingModel) sender;
            double totalprice;

            if (tm.AssetId > 0)
                totalprice = tm.Price;
            else
                totalprice = tm.Price;

            if (totalprice <= viewStream.MoneyValues)
                viewMarket.ConfirmWithBank(tm, true);
            else
                viewMarket.ConfirmWithBank(tm, false);
        }

        private void AddView(String name)
        {
            bool exist = false;
            for (int i = 0; i < _views.Count; i++)
                if (_views[i] == name)
                    exist = true;

            if (!exist)
                _views.Add(name);
        }

        private void ShowAllViews(object sender, EventArgs e)
        {
            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i] == "sviAssetWindow")
                    sviAssetWindow.Visibility = Visibility.Visible;
                else if (_views[i] == "sviHistoryWindow")
                    sviHistoryWindow.Visibility = Visibility.Visible;
                else if (_views[i] == "sviItemWindow")
                    sviItemWindow.Visibility = Visibility.Visible;
                else if (_views[i] == "sviMarketWindow")
                    sviMarketWindow.Visibility = Visibility.Visible;
            }
            btnViews.Visibility = Visibility.Collapsed;
            btnViewsHide.Visibility = Visibility.Visible;
        }

        private void HideAll(object sender, EventArgs e)
        {
            sviAssetWindow.Visibility = Visibility.Collapsed;
            sviHistoryWindow.Visibility = Visibility.Collapsed;
            sviItemWindow.Visibility = Visibility.Collapsed;
            sviMarketWindow.Visibility = Visibility.Collapsed;
            btnViews.Visibility = Visibility.Visible;
            btnViewsHide.Visibility = Visibility.Collapsed;
        }

        private void Handle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rekt.Fill = Brushes.AliceBlue;

            if (sviAssetWindow.IsMouseOver)
                sviAssetWindow.Visibility = Visibility.Collapsed; //.IsMouseDirectlyOver(20,20);
        }

        /// <summary>
        ///     Requests group number from server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_groupsSelectedValueChanged(object sender, EventArgs e)
        {
            var selected = (GroupModel) sender;

            _client.SendGroupnumber(selected.Id);
        }

        private void ExpandMenuBar(object sender, EventArgs eventArgs)
        {
            const double newOrientation = 0;
            double orientation = menuBar.ActualOrientation;

            var d = new Duration(TimeSpan.FromMilliseconds(300));
            var da = new DoubleAnimation(orientation, newOrientation, d);
            var sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(da, menuBar);
            Storyboard.SetTargetProperty(da, new PropertyPath("Orientation"));
            sb.FillBehavior = FillBehavior.Stop;

            sb.Begin();
            menuBar.Orientation = newOrientation;

            double y = menuBar.Center.Y;

            var point = new Point(menuBar.Center.X, menuBar.Center.Y);
            point.X = 360;
            if (y < 281)
                point.Y = 530;

            else
                point.Y = 260;

            var da2 = new PointAnimation(menuBar.ActualCenter, point, d);

            var sb2 = new Storyboard();
            sb2.Children.Add(da2);

            Storyboard.SetTarget(da2, menuBar);
            Storyboard.SetTargetProperty(da2, new PropertyPath("Center"));
            sb2.FillBehavior = FillBehavior.Stop;

            sb2.Begin();
            menuBar.Center = point;
        }
    }
}