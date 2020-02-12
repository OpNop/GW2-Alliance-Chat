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
using Gw2Sharp.Mumble;
using System.Threading;

delegate void Message(string time, string from, string message);

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TcpClient client;
        private string name = "TINY Member";
        private bool _autoScroll = true;

        public readonly Game game;
        public readonly Mumble mumble;
        public readonly Dictionary<int, Map> maps;

        public MainWindow()
        {
            InitializeComponent();

            //Check for running Game
            game = new Game();
            game.Load();
            
            //Exit if game is not running 
            //Temporary untill we can watch for game
            if (!game.IsRunning) { Environment.Exit(0); }

            //Load Map service
            maps = Map.Load();

            //Load Mumble
            mumble = new Mumble();
            mumble.Init();
            mumble.HookGame();

            Task.Run(() => mumble.UpdateMumble());

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
            //WriteToChat("Enter your name to start chatting.");
            
            if (mumble.MumbleData.CharacterName != "")
            {
                client.Connect();
            }


            Task.Run(() => updateCharacter());
        }

        private void updateCharacter()
        {
            while (true)
            {
                var packet = new
                {
                    type            = PacketType.UPDATE,
                    name            = mumble.MumbleData.CharacterName,
                    commander       = mumble.MumbleData.IsCommander,
                    race            = mumble.MumbleData.Race,
                    prof            = mumble.MumbleData.Profession,
                    spec            = EliteSpec.GetElite(mumble.MumbleData.Specialization),
                    map             = maps[mumble.MumbleData.MapId].MapName,
                    position        = new
                                    {
                                        X = $"{mumble.MumbleData.AvatarPosition.X:N6}",
                                        Y = $"{mumble.MumbleData.AvatarPosition.Y:N6}",
                                        Z = $"{mumble.MumbleData.AvatarPosition.Z:N6}"
                                    },
                server_address  = mumble.MumbleData.ServerAddress
                };
                client.Send(JsonConvert.SerializeObject(packet));
                //WriteToChat($"Position:   X: {mumble.MumbleData.AvatarPosition.X:N6} Y: {mumble.MumbleData.AvatarPosition.Y:N6} Z: {mumble.MumbleData.AvatarPosition.Z:N6}");
                Thread.Sleep(500);
            }
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

        async Task UpdateMumble()
        {
            //check if game is running 
            

            //check if Mumble is running

            //update
        }

        private void WriteToChat(string time, string from, string message)
        {
            ChatBox.Dispatcher.BeginInvoke(new Message((time, from, message) =>
            {
                ChatBox.AppendText($"[{time}] ", Brushes.Gray);
                ChatBox.AppendText($"{from}: ", Brushes.DarkTurquoise);
                ChatBox.AppendText(message, Brushes.PowderBlue);
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

                ////check if we are connected
                //if(!client.IsConnected)
                //{
                //    //name = message.Text;
                //    //connect to server
                //    client.Connect();
                //} else
                //{
                    //send packet as nomral
                    var packet = new
                    {
                        type = PacketType.MESSAGE,
                        //name = mumble.MumbleData.CharacterName,
                        message = message.Text
                    };
                    client.Send(JsonConvert.SerializeObject(packet));
                //}
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
