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
using System.Runtime.InteropServices;


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
        private bool _debugMode = false;

        public readonly Game game;
        public readonly Mumble mumble;
        public readonly Dictionary<int, Map> maps;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public MainWindow()
        {
            //Debug console
            AllocConsole();

            InitializeComponent();
            Console.WriteLine("About to check for game");

            //Check for running Game
            game = new Game();
            game.Load();
            
            //Exit if game is not running 
            //Temporary untill we can watch for game
            if (!game.IsRunning) {
                var startDebugMode = MessageBox.Show("Game not detected, do you want to load in test client mode?", "Game not running", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(startDebugMode == MessageBoxResult.Yes)
                {
                    _debugMode = true;
                } else
                {
                    Environment.Exit(0);
                }
            }

            //Load Map service
            maps = Map.Load();

            //Load Mumble
            if (!_debugMode)
            {
                mumble = new Mumble();
                mumble.Init();
                mumble.HookGame();

                Task.Run(() => mumble.UpdateMumble());
            }

            string[] args = Environment.GetCommandLineArgs();

            //Setup Client
            string address = "67.61.134.200";
            if (args.Length >= 2)
            {
                address = args[1];
                Console.WriteLine($"Using Server IP: {address}");
            }

            int port = 8888;
            if (args.Length == 3)
            {
                port = int.Parse(args[2]);
                Console.WriteLine($"Using Server Port: {port}");
            }


            client = new TcpClient(address, port, false, null, null)
            {
                Connected = ServerConnected,
                Disconnected = ServerDisconnected,
                DataReceived = MessageReceived
            };

            //Send default messages
            WriteToChat("==TINY Alliance Chat System==");
            WriteToChat("==          Version Beta 1         ==");
            WriteToChat("");
            
            if (_debugMode || mumble.MumbleData.CharacterName != "")
            {
                try
                {
                    client.Connect();
                }
                catch (Exception)
                {
                    WriteToChat("Can not connect to server, retrying in 5 seconds");
                }
                
            }


            
        }

        private void updateCharacter()
        {
            var debugMumble = new
            {
                MumbleData = new
                {
                    CharacterName = "Test Client " + DateTime.Now.ToString("hhmmss"),
                    IsCommander = false,
                    Race = "Asura",
                    Profession = "test Profession",
                    Specialization = 34,
                    MapId = 18,
                    AvatarPosition = new
                    {
                        X = 0,
                        Y = 0,
                        Z = 0
                    },
                    ServerAddress = "0.0.0.0"

                }
            };

            
            while (true)
            {

                object packet;

                if (_debugMode)
                {
                    packet = new
                    {
                        type = PacketType.UPDATE,
                        name = debugMumble.MumbleData.CharacterName,
                        commander = debugMumble.MumbleData.IsCommander,
                        race = debugMumble.MumbleData.Race,
                        prof = debugMumble.MumbleData.Profession,
                        spec = EliteSpec.GetElite(debugMumble.MumbleData.Specialization),
                        map = maps[debugMumble.MumbleData.MapId].MapName,
                        position = new
                        {
                            X = $"{debugMumble.MumbleData.AvatarPosition.X:N6}",
                            Y = $"{debugMumble.MumbleData.AvatarPosition.Y:N6}",
                            Z = $"{debugMumble.MumbleData.AvatarPosition.Z:N6}"
                        },
                        server_address = debugMumble.MumbleData.ServerAddress
                    };
                }
                else
                {
                    packet = new
                    {
                        type = PacketType.UPDATE,
                        name = mumble.MumbleData.CharacterName,
                        commander = mumble.MumbleData.IsCommander,
                        race = mumble.MumbleData.Race,
                        prof = mumble.MumbleData.Profession,
                        spec = EliteSpec.GetElite(mumble.MumbleData.Specialization),
                        map = maps[mumble.MumbleData.MapId].MapName,
                        position = new
                        {
                            X = $"{mumble.MumbleData.AvatarPosition.X:N6}",
                            Y = $"{mumble.MumbleData.AvatarPosition.Y:N6}",
                            Z = $"{mumble.MumbleData.AvatarPosition.Z:N6}"
                        },
                        server_address = mumble.MumbleData.ServerAddress
                    };
                }
                if (client.IsConnected)
                {
                    client.Send(JsonConvert.SerializeObject(packet));
                    Thread.Sleep(500);
                } else
                {
                    return;
                }
            }
        }

        async Task MessageReceived(byte[] data)
        {
            var rawData = Encoding.UTF8.GetString(data);
            //check for multiple packets
            var rawPackets = rawData.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawPacket in rawPackets)
            {
                Console.WriteLine($"RECV> {rawPacket}");
                dynamic packet;
                //Try and parse JSON
                try
                {
                    packet = JsonConvert.DeserializeObject<dynamic>(rawPacket);
                    switch ((PacketType)packet.type)
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

            
        }

        async Task ServerConnected()
        {
            WriteToChat("Server connected");

            await Task.Run(() => updateCharacter());
        }

        async Task ServerDisconnected()
        {
            WriteToChat("Server disconnected, Retrying in 5 seconds -- But in reality I wont, need to fix this.");
            while (client.IsConnected == false)
            {
                Thread.Sleep(5000);
                client.Connect();
            }
        }

        async Task UpdateMumble()
        {
            //check if game is running 
            

            //check if Mumble is running

            //update
        }

        private void WriteToChat(string time, string from, string message)
        {
            ChatBox.Dispatcher.Invoke(() =>
            {
                ChatBox.AppendText($"[{time}] ", Brushes.Gray);
                ChatBox.AppendText($"{from}: ", Brushes.DarkTurquoise);
                ChatBox.AppendText(message, Brushes.PowderBlue);
                ChatBox.AppendText(Environment.NewLine);
                ChatBox.ScrollToEnd();
            });
        }

        private void WriteToChat(string message)
        {
            ChatBox.Dispatcher.Invoke(() =>
            {
                var time = DateTime.Now.ToString("h:mm tt");
                ChatBox.AppendText($"[{time}] ", Brushes.Gray);
                ChatBox.AppendText(message, Brushes.DarkGray);
                ChatBox.AppendText(Environment.NewLine);
                ChatBox.ScrollToEnd();
            });
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
                //send packet as nomral
                var packet = new
                {
                    type = PacketType.MESSAGE,
                    message = message.Text
                };
                client.Send(JsonConvert.SerializeObject(packet));

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
