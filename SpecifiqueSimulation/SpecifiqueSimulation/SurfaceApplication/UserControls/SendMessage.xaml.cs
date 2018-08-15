using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for SendMessage.xaml
    /// </summary>
    public partial class SendMessage
    {
        //Storing variables
        public List<GroupModel> Groups;
        public MessageModel Outgoing;
        private string _subject;
        private string _text;

        //Event Handlers

        public SendMessage()
        {
            InitializeComponent();
            Groups = new List<GroupModel>();
        }

        public event EventHandler SendButtonClicked;

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
            groupsListBox.DataContext = Groups;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _subject = subjectTextBox.Text;
            _text = textTextBox.Text;
            var selected = (GroupModel) groupsListBox.SelectedItem;

            if (groupsListBox.SelectedItem != null)
            {
                var dt = new DateTime();
                Outgoing = new MessageModel(0, 0, selected.Id,"","", 0, _subject, _text, dt);

                EventHandler handler = SendButtonClicked;
                if (handler != null)
                    handler(sender, e);

                subjectTextBox.Text = "Write a subject";
                textTextBox.Text = "Write a new message...";
                groupsListBox.SelectedIndex = -1;
            }
            else
                noGroupTxt.Visibility = Visibility.Visible;
        }

        public void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) sender;
            tb.Text = string.Empty;
            tb.GotFocus -= TextBox_GotFocus;
        }

        private void SelectedGroup(object sender, RoutedEventArgs e)
        {
            noGroupTxt.Visibility = Visibility.Collapsed;
        }
    }
}