using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Model;

namespace SurfaceApplication.Views
{
    /// <summary>
    /// Interaction logic for ViewMessage.xaml
    /// </summary>
    public partial class ViewMessage : UserControl
    {
        public string txt;
        public string viewMessageSubject;
        public string tm;
        public string t;
        public string f;
        public MessageModel message;

        public ViewMessage()
        {
            // <Henter messageModel>
            message = new MessageModel(0, 0, "Hei", "Dette er brødteksten.", "25.10.2012", 1, 2);
            viewMessageSubject = "HAHAAH"; //message.subject;
            subjextTextBox.Text = "JOJDSA";

            InitializeComponent();
        }
    }
}
