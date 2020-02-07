using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WriteToChat("==TINY Alliance Chat System==");
            WriteToChat("==          Version Beta 1         ==");
        }
        private void WriteToChat(string time, string from, string message)
        {
            ChatBox.AppendText($"[{time}] ", Brushes.Gray);
            ChatBox.AppendText($"{from}: ", Brushes.YellowGreen);
            ChatBox.AppendText(message, Brushes.White);
            ChatBox.AppendText(Environment.NewLine);
            ChatBox.ScrollToEnd();
        }

        private void WriteToChat(string message)
        {
            ChatBox.AppendText(message, Brushes.Gray);
            ChatBox.AppendText(Environment.NewLine);
            ChatBox.ScrollToEnd();
        }

        private bool _autoScroll = true;
        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {
                _autoScroll = ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight;
            }

            if (_autoScroll && e.ExtentHeightChange != 0)
            {
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ExtentHeight);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                WriteToChat(DateTime.Now.ToShortTimeString(), "Tiny Taimi", message.Text);
                message.Text = "";
            }
        }
    }
}
