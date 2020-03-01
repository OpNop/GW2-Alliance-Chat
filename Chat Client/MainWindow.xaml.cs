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
using DLG.ToolBox.Log;
using System.Net;
using System.IO;
using Chat_Client.utils;

delegate void Message(string time, string from, string message);

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
#if DEBUG
        private string serverAddr = "chat.noobnetwork.net";
        private int serverPort = 8888;
#else
        private string serverAddr = "chat.jumpsfor.science";
        private int serverPort = 8888;
#endif

        private TcpClient client;
        private bool _autoScroll = true;
        private bool _debugMode = false;

        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        public readonly Game game = new Game();
        public Mumble mumble;
        public string APIKey;
        private static readonly Logger _log = Logger.getInstance();

        public MainWindow()
        {
            InitializeComponent();

            //Parse Arguments
            ParseArguments();

            //Init logger class
            _log.Initialize("TACS_", "", "./log", LogIntervalType.IT_PER_DAY, LogLevel.D, true, true, true);

            //Setup Tray Icon
            SetupNotifyIcon();
        }

        private void ParseArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            Arguments CommandLine = new Arguments(args);

            if (CommandLine["?"] != null || CommandLine["help"] != null)
            {
                Console.WriteLine("Options:");
                Console.WriteLine("  --help\tShows this message");
                Console.WriteLine("  --server\tSets the chat server to connect to");
                Console.WriteLine("  --port\tSets the port to use for the chat server");
                Console.WriteLine("  --debug\tStarts the client in debug mode");
                Environment.Exit(0);
            }
                        
            // --server
            if(CommandLine["server"] != null)
            {
                serverAddr = CommandLine["server"];
                _log.AddInfo($"Using Server IP: {serverAddr}");
            }

            // --port
            if(CommandLine["port"] != null)
            {
                serverPort = int.Parse(CommandLine["port"]);
                _log.AddInfo($"Using Server Port: {serverPort}");
            }

            // --debug
            if(CommandLine["debug"] != null)
            {
                _debugMode = true;
                _log.AddWarning("Starting in Debug Mode");
            }
        }

        private void CheckForAPIKey()
        {
            APIKey = Properties.Settings.Default.apiKey;
            if (APIKey.Length == 0)
            {
                ShowKeyDialog();
            }
        }

        private void ShowKeyDialog()
        {
            var keyWindow = new APIKey();
            keyWindow.Owner = this;
            keyWindow.ShowDialog();

            //Reload Key
            APIKey = Properties.Settings.Default.apiKey;
        }

        private void GameStateChanged(object sender, GameStateChangedArgs e)
        {
            _log.AddNotice($"Game State is: {e.GameState}");
            switch (e.GameState)
            {
                case GameState.NotRunning:
                    //Stop Mumble Watcher
                    mumble.StopMumbleRefresh();
                    break;
                case GameState.Launcher:
                    //IDK?
                    //Stop the mumble update also I guess
                    mumble.StopMumbleRefresh();
                    break;
                case GameState.InGame:
                    //Start Mumble Watcher
                    mumble.StartMumbleRefresh();
                    break;
            }
        }

        private void SetupNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Assets/tiny_icon.ico")).Stream);
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Tiny Alliance Chat Service (TACS)";

            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Show Chat").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            this.Show();
        }

        private void IsMapShowing(object sender, MapStatusChangedArgs e)
        {
            if (e.IsMapOpen)
            {
                Dispatcher.Invoke(() =>
                {
                    Hide();
                });
            } else
            {
                Dispatcher.Invoke(() =>
                {
                    Show();
                });
            }
        }

        //private void UpdateCharacter()
        //{
        //    var debugMumble = new
        //    {
        //        MumbleData = new
        //        {
        //            CharacterName = "Test Client " + DateTime.Now.ToString("hhmmss"),
        //            IsCommander = false,
        //            Race = "Asura",
        //            Profession = "test Profession",
        //            Specialization = 34,
        //            MapId = 18,
        //            AvatarPosition = new
        //            {
        //                X = 0,
        //                Y = 0,
        //                Z = 0
        //            },
        //            ServerAddress = "0.0.0.0"

        //        }
        //    };

            
        //    while (true)
        //    {

        //        object packet;

        //        if (_debugMode)
        //        {
        //            packet = new
        //            {
        //                type = PacketType.UPDATE,
        //                name = debugMumble.MumbleData.CharacterName,
        //                commander = debugMumble.MumbleData.IsCommander,
        //                race = debugMumble.MumbleData.Race,
        //                prof = debugMumble.MumbleData.Profession,
        //                spec = EliteSpec.GetElite(debugMumble.MumbleData.Specialization),
        //                //map = maps[debugMumble.MumbleData.MapId].MapName,
        //                map = debugMumble.MumbleData.MapId,
        //                position = new
        //                {
        //                    X = $"{debugMumble.MumbleData.AvatarPosition.X:N6}",
        //                    Y = $"{debugMumble.MumbleData.AvatarPosition.Y:N6}",
        //                    Z = $"{debugMumble.MumbleData.AvatarPosition.Z:N6}"
        //                },
        //                server_address = debugMumble.MumbleData.ServerAddress
        //            };
        //        }
        //        else
        //        {
        //            packet = new
        //            {
        //                type = PacketType.UPDATE,
        //                name = mumble.MumbleData.CharacterName,
        //                commander = mumble.MumbleData.IsCommander,
        //                race = mumble.MumbleData.Race,
        //                prof = mumble.MumbleData.Profession,
        //                spec = EliteSpec.GetElite(mumble.MumbleData.Specialization),
        //                //map = maps[mumble.MumbleData.MapId].MapName,
        //                map = mumble.MumbleData.MapId,
        //                position = new
        //                {
        //                    X = $"{mumble.MumbleData.AvatarPosition.X:N6}",
        //                    Y = $"{mumble.MumbleData.AvatarPosition.Y:N6}",
        //                    Z = $"{mumble.MumbleData.AvatarPosition.Z:N6}"
        //                },
        //                server_address = mumble.MumbleData.ServerAddress
        //            };
        //        }
        //        if (client.IsConnected)
        //        {
        //            client.Send(JsonConvert.SerializeObject(packet));
        //            Thread.Sleep(500);
        //        } else
        //        {
        //            return;
        //        }
        //    }
        //}

        async void MessageReceived(object sender, DataReceivedFromServerEventArgs e)
        {
            var rawData = Encoding.UTF8.GetString(e.Data);
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
                catch (Exception ex)
                {
                    WriteToChat(ex.Message);
                }
            }

            
        }

        async void ServerConnected(object sender, EventArgs e)
        {
            WriteToChat("Server connected");
            if (Properties.Settings.Default.apiKey.Length > 0)
            {
                client.Send(JsonConvert.SerializeObject(new
                {
                    type = PacketType.ENTER,
                    key = Properties.Settings.Default.apiKey
                }));
            }
        }

        private void ServerDisconnected(object sender, EventArgs e)
        {
            WriteToChat("Server disconnected, Retrying in 5 seconds -- But in reality I wont, need to fix this.");
            while (client.IsConnected == false)
            {
                Thread.Sleep(5000);
                client.Connect();
            }
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
                if (message.Text == "/setkey")
                {
                    ShowKeyDialog();
                }
                else
                {
                    //send packet as nomral
                    var packet = new
                    {
                        type = PacketType.MESSAGE,
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

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnExit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;

            base.OnSourceInitialized(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Save window location and size
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Save();
            if (!_isExit)
            {
                e.Cancel = true;
                Hide();
            }

            //Really quitting stop the watchers
            mumble.StopMumbleRefresh();
            game.StopWatch();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Check For API key
            CheckForAPIKey();

            //Start watching for game
            game.GameStateChanged += GameStateChanged;
            game.StartWatch();

            mumble = new Mumble();
            mumble.MapStatusChanged += IsMapShowing;
            mumble.MumbleUpdated += OnMumbleUpdated;

            _log.AddInfo($"Connecting to {serverAddr}:{serverPort}");
            client = new TcpClient(serverAddr, serverPort, false, null, null);
            client.Connected += ServerConnected;
            client.Disconnected += ServerDisconnected;
            client.DataReceived += MessageReceived;

            //Send default messages
            WriteToChat("==TINY Alliance Chat System==");
            WriteToChat("==          Version Beta 1         ==");
            WriteToChat("");

            //if (_debugMode || mumble.MumbleData.CharacterName != "")
            //{
                try
                {
                    client.Connect();
                }
                catch (Exception)
                {
                    WriteToChat("Can not connect to server, retrying in 5 seconds");
                }

            //}
        }

        private void OnMumbleUpdated(object sender, MumbleUpdatedArgs mumble)
        {
            //If connected to the server
            if (client.IsConnected)
            {
                //Send update packet
                var packet = new
                {
                    type = PacketType.UPDATE,
                    name = mumble.MumbleData.CharacterName,
                    commander = mumble.MumbleData.IsCommander,
                    race = mumble.MumbleData.Race,
                    prof = mumble.MumbleData.Profession,
                    spec = EliteSpec.GetElite(mumble.MumbleData.Specialization),
                    map = mumble.MumbleData.MapId,
                    position = new
                    {
                        X = $"{mumble.MumbleData.AvatarPosition.X:N6}",
                        Y = $"{mumble.MumbleData.AvatarPosition.Y:N6}",
                        Z = $"{mumble.MumbleData.AvatarPosition.Z:N6}"
                    },
                    server_address = mumble.MumbleData.ServerAddress
                };

                client.Send(JsonConvert.SerializeObject(packet));
            }
        }
    }
}
