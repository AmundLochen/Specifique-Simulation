using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for MessageHistory.xaml
    /// </summary>
    public partial class MessageHistory
    {
        //List<TradingModel> tradingConvers { get; set; }
        public MessageModel Outgoing;
        //public event EventHandler tradeAnswered;
        //public event EventHandler exitButtonPressed;
        //public CashPerTeamModel myCash; //må hentes


        public MessageHistory()
        {
            InitializeComponent();
        }

        private List<MessageModel> Conversation { get; set; }
        public event EventHandler SendMessageClicked;

        public void UpdateConversation(MessageModel mes)
        {
            if (mes.ParentId == Conversation[0].ParentId)
                Conversation.Add(mes);
            Dispatcher.Invoke((Action) (() =>
                {
                    listConversation.DataContext = Conversation;
                    listConversation.Items.Refresh();
                    if (listConversation.Items.Count - 1 >= 0)
                        listConversation.ScrollIntoView(listConversation.Items[listConversation.Items.Count - 1]);
                }));
        }

        public void ShowMessageHistory(List<MessageModel> con)
        {
            Conversation = con;
            tbSub.Text = Conversation[0].Subject;
            listConversation.Visibility = Visibility.Visible;
            borderMsg.Visibility = Visibility.Visible;
            typeMessagetxt.Text = "Message history";
            Dispatcher.Invoke((Action) (() =>
                {
                    listConversation.DataContext = Conversation;
                    if (listConversation.Items.Count - 1 >= 0)
                        listConversation.ScrollIntoView(listConversation.Items[listConversation.Items.Count - 1]);
                }));
            MessageText.Text = "Write a message...";
            MessageText.GotFocus += TextBox_GotFocus;
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            int parentid = Conversation[0].ParentId;
            String message = MessageText.Text;
            var dt = new DateTime();
            Outgoing = new MessageModel(0, 0, 0,"","", parentid, "", message, dt);
            MessageText.Text = "Write a message...";

            EventHandler handler = SendMessageClicked;
            if (handler != null)
                handler(sender, e);

            MessageModel localModel = Outgoing;
            localModel.Subject = Conversation[0].Subject;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) sender;
            tb.Text = string.Empty;
            tb.GotFocus -= TextBox_GotFocus;
        }
    }
}