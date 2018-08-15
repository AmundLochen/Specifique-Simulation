using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Inbox.xaml
    /// </summary>
    public partial class Inbox
    {
        //Stored Variables
        public static readonly DependencyProperty InboxProperty = DependencyProperty.Register(
            "Inbox", typeof (String), typeof (Inbox));

        public int YourGroup;

        public Inbox()
        {
            InitializeComponent();
            Messages = new List<MessageModel>();
            Trades = new List<TradingModel>();
            listBoxMessage.TouchDown += StackPanel_TouchDown;
        }

        private List<MessageModel> Messages { get; set; }
        private List<MessageModel> Conversation { get; set; }
        private List<TradingModel> Trades { get; set; }
        private List<TradingModel> TradingConvers { get; set; }

        public List<MessageModel> GetMessages
        {
            get { return Conversation; }
        }

        public List<TradingModel> GetTrades
        {
            get { return TradingConvers; }
        }

        //Event handlers
        public event EventHandler TradeAnswered;
        public event EventHandler ShowMessageHistory;
        public event EventHandler ShowMessageNotification;

        /// <summary>
        ///     Returns expired trades.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<TradingModel> CheckForExpiredTrades(DateTime date)
        {
            if (Trades == null)
                return null;

            var expiredTrades = new List<TradingModel>();

            foreach (TradingModel trade in Trades)
                if (date >= trade.Time && trade.Accept == 0)
                {
                    trade.Accept = 3;
                    expiredTrades.Add(trade);
                }

            ShowMessages();
            return expiredTrades;
        }

        private void ShowNotificationIcon(object recieved, EventArgs e)
        {
            EventHandler handler = ShowMessageNotification;
            if (handler != null)
                handler(recieved, e);
        }

        private void AddTrades()
        {
            ShowNotificationIcon("test", null);
            groupid.Text = YourGroup.ToString();
            lblCantAfford.Visibility = Visibility.Collapsed;
        }


        public void AddInboxMessage(MessageModel message)
        {
            //Setting your groupid to 9 to indicate that it is your group in the view. Returned to right value when needed
            if (message.From == YourGroup)
                message.From = 9;
            else
                message.To = 9;
            Messages.Add(message);

            if(message.From != 9)
                ShowNotificationIcon("test", null);
            ShowMessages();
            //groupid.Text = yourGroup.ToString();
        }

        public void AddInboxTrade(TradingModel message)
        {
            bool existing = false;
            for (int i = 0; i < Trades.Count; i++)
                if (Trades[i].Id == message.Id)
                {
                    Trades[i].Accept = message.Accept;
                    existing = true;
                }
            if (!existing)
                Trades.Add(message);

            Trades = Trades.OrderByDescending(o => o.Time).ToList();
            ShowNotificationIcon("test", null);
            ShowMessages();
        }

        public void TrySetDataContext()
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
            listBoxMessage.DataContext = Messages;
        }

        private void ShowMessages()
        {
            Dispatcher.Invoke((Action) (() =>
                {
                    listBoxMessage.DataContext = SortMessages();
                    listBoxMessage.Items.Refresh();
                    if (listBoxMessage.Items.Count - 1 >= 0)
                        listBoxMessage.ScrollIntoView(listBoxMessage.Items[0]);
                    //System.Windows.Controls.ListViewItem lv = (System.Windows.Controls.ListViewItem) listBoxMessage.Items.CurrentItem;
                    //this.listBoxMessage.Items.Culture.Parent =  true;
                    //lv.Background = Brushes.AliceBlue;
                    //listBoxMessage.Items = lv;
                }));

            Dispatcher.Invoke((Action) (() =>
                {
                    listBoxTrading.DataContext = Trades;
                    listBoxTrading.Items.Refresh();
                    if (listBoxTrading.Items.Count - 1 >= 0)
                        listBoxTrading.ScrollIntoView(listBoxTrading.Items[0]);
                }));
        }

        private List<MessageModel> SortMessages()
        {
            var tempMessage = new List<MessageModel>();
            for (int i = Messages.Count - 1; i >= 0; i--)
            {
                if (tempMessage.Count == 0)
                    tempMessage.Add(Messages[i]);

                bool lik = false;
                for (int t = 0; t < tempMessage.Count; t++)
                {
                    if (Messages[i].ParentId.Equals(tempMessage[t].ParentId))
                    {
                        lik = true;
                    }
                }
                if (!lik)
                    tempMessage.Add(Messages[i]);
            }
            return tempMessage;
        }

        private List<TradingModel> SortTraid()
        {
            return Trades;
        }


        private void StackPanel_TouchDown(object sender, TouchEventArgs e)
        {
            var idPanel = (StackPanel) sender;
            var m = (MessageModel) idPanel.DataContext;
            Conversation = new List<MessageModel>();
            int id = Convert.ToInt32(m.ParentId);

            foreach (MessageModel element in Messages)
            {
                if (id == element.ParentId)
                {
                    if (element.From == 9)
                        element.From = YourGroup; //Returning correct groupvalue to codebehind
                    else
                        element.To = YourGroup;
                    Conversation.Add(element);
                }
            }
            EventHandler handler = ShowMessageHistory;
            if (handler != null)
                handler(sender, e);
        }

        private void StackPanel_MouseDown(object sender, EventArgs e)
        {
            var idPanel = (StackPanel) sender;
            var m = (MessageModel) idPanel.DataContext;
            Conversation = new List<MessageModel>();

            int id = Convert.ToInt32(m.ParentId);
            foreach (MessageModel element in Messages)
            {
                if (id == element.ParentId)
                {
                    Conversation.Add(element);
                }
            }
            EventHandler handler = ShowMessageHistory;
            if (handler != null)
                handler(sender, e);
        }

        public void TradeChanged(int tradingId)
        {
            lblCantAfford.Visibility = Visibility.Collapsed;
            foreach (TradingModel e in Trades)
                if (e.Id == tradingId)
                {
                    TradingConvers[TradingConvers.Count].Accept = 3;
                    listBoxTrading.DataContext = SortMessages();
                    break;
                }
        }

        private void CommonBtnClick(object sender, RoutedEventArgs e)
        {
            var idPanel = (StackPanel) sender;
            var m = (TradingModel) idPanel.DataContext;

            foreach (TradingModel trade in Trades)
            {
                if (trade == m)
                {
                    var feSource = e.Source as FrameworkElement;
                    if (feSource != null)
                        switch (feSource.Name)
                        {
                            case "yesBtn":
                                {
                                    trade.Accept = 2;
                                    m = trade;
                                    break;
                                }
                            case "noBtn":
                                {
                                    trade.Accept = 1;
                                    m = trade;
                                    break;
                                }
                        }
                }
            }
            ShowMessages();
            EventHandler handlertwo = TradeAnswered;
            if (handlertwo != null)
                handlertwo(m, e);
        }
    }
}