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
using SimpleTcp;
using Newtonsoft.Json;

delegate void Message(string time, string from, string message);

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private string name = "TINY Member";

        public MainWindow()
        {
            InitializeComponent();

            //Setup Client
            client = new TcpClient("67.61.134.200", 8888, false, null, null)
            {
                Connected = ServerConnected,
                Disconnected = ServerDisconnected,
                DataReceived = MessageReceived
            };

            //Send default messages
            WriteToChat("==TINY Alliance Chat System==");
            WriteToChat("==          Version Beta 1         ==");
            WriteToChat("");
            WriteToChat("Enter your name to start chatting.");
        }
       
        async Task MessageReceived(byte[] data)
        {
            var packetRaw = Encoding.UTF8.GetString(data);
            dynamic packet;
            //Try and parse JSON
            try
            {
                packet = JsonConvert.DeserializeObject<dynamic>(packetRaw);
                switch ((PacketType) packet.type)
                {
                    case PacketType.ENTER:
                        //Handle ENTER message
                        break;
                    case PacketType.LEAVE:
                        //Handle LEAVE message
                        break;
                    case PacketType.MESSAGE:
                        WriteToChat(DateTime.Now.ToString("h:mm tt"), (string)packet.name, (string)packet.message);
                        break;
                    case PacketType.SYSTEM:
                        WriteToChat((string)packet.message);
                        break;
                    case PacketType.UPDATE:
                        //Handle location update
                        break;
                    default:
                        throw new Exception("Missing packet type");
                }
            }
            catch (Exception e)
            {
                WriteToChat(e.Message);
            }
        }

        async Task ServerConnected()
        {
            WriteToChat("Server connected");
            //Name Change
            var packet = new
            {
                type = PacketType.MESSAGE,
                name = name,
                message = $"/name {name}"
            };
            //client.SendAsync(message.Text);
            client.Send(JsonConvert.SerializeObject(packet));
        }

        async Task ServerDisconnected()
        {
            WriteToChat("Server disconnected");
        }

        private void WriteToChat(string time, string from, string message)
        {
            ChatBox.Dispatcher.BeginInvoke(new Message((time, from, message) =>
            {
                ChatBox.AppendText($"[{time}] ", Brushes.Gray);
                ChatBox.AppendText($"{from}: ", Brushes.LightSeaGreen);
                ChatBox.AppendText(message, Brushes.White);
                ChatBox.AppendText(Environment.NewLine);
                ChatBox.ScrollToEnd();
            }), new object[] { time, from, message });

        }

        private void WriteToChat(string message)
        {
            ChatBox.Dispatcher.BeginInvoke(new Action<string>((message) =>
            {
                ChatBox.AppendText(message, Brushes.Gray);
                ChatBox.AppendText(Environment.NewLine);
                ChatBox.ScrollToEnd();
            }), new object[] { message });
            
        }

        public void onMessage(string message)
        {

            WriteToChat(message);
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
            if (e.Key == Key.Return && message.Text != "")
            {

                //check if we are connected
                if(!client.IsConnected)
                {
                    name = message.Text;
                    //connect to server
                    client.Connect();
                } else
                {
                    //send packet as nomral
                    var packet = new
                    {
                        type = PacketType.MESSAGE,
                        name = name,
                        message = message.Text
                    };
                    client.Send(JsonConvert.SerializeObject(packet));
                }
                message.Text = "";
            }
        }

        private enum PacketType
        {
            MESSAGE,
            ENTER,
            LEAVE,
            SYSTEM,
            UPDATE
        }
    }
}
