using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Model;

namespace SurfaceApplication.UserControls
{
    /// <summary>
    ///     Interaction logic for Start.xaml
    /// </summary>
    public partial class Start
    {
        private List<GroupModel> _groups;

        public Start()
        {
            _groups = new List<GroupModel>();
            InitializeComponent();
            SetDataContext();
        }

        public event EventHandler GroupsSelectedValueChanged;

        public void SetGroups(List<GroupModel> newGroups)
        {
            _groups = newGroups;
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

        public void SetDataContext()
        {
            groupsListBox.DataContext = _groups;
        }

        public void SetInfoLabel()
        {
            Dispatcher.Invoke((Action) (() =>
                {
                    var selected = (GroupModel) groupsListBox.SelectedItem;
                    groupsListBox.Visibility = Visibility.Visible;
                    infoLabel.Visibility = Visibility.Visible;
                    infoLabel.Content = selected.Name + " was taken. Selected a different group.";
                    infoLabel.Foreground = Brushes.Red;
                    groupsListBox.SelectedIndex = -1;
                }));
        }

        private void SelectedGroup(object sender, SelectionChangedEventArgs e)
        {
            if (groupsListBox.SelectedIndex == -1)
                return;

            groupsListBox.Visibility = Visibility.Hidden;
            infoLabel.Visibility = Visibility.Hidden;

            sender = groupsListBox.SelectedItem;

            EventHandler handler = GroupsSelectedValueChanged;
            if (handler != null)
                handler(sender, e);
        }
    }
}