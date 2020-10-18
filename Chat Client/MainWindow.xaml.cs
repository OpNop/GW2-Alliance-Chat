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
using System.Threading;
using DLG.ToolBox.Log;
using Chat_Client.utils;
using Chat_Client.Packets;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Gw2Sharp;
using System.Runtime.InteropServices.WindowsRuntime;

delegate void ChatMessage(string time, string from, string message);

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
#if DEBUG
        private string serverAddr = "127.0.0.1";
        private int serverPort = 8888;
#else
        private string serverAddr = "chat.jumpsfor.science";
        private int serverPort = 8888;
#endif

        private TcpClient client;
        private State clientState = State.Disconnected;
        private bool _autoScroll = true;
        private bool _debugMode = false;
        private string _mumbleFile = "MumbleLink";

        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        public Game game;
        public Mumble mumble;
        public DiscordRichPresence discord;
        public string apiKey;
        private static readonly Logger _log = Logger.getInstance();

        //toogle Stay Open
        private bool stayOpenOnMap;
        private int hookId;

        public MainWindow()
        {
            InitializeComponent();

            //Load Settings
            if (Properties.Settings.Default.needUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.needUpgrade = false;
                Properties.Settings.Default.Save();
            }
            stayOpenOnMap = Properties.Settings.Default.stayOpenOnMap;
            apiKey = Properties.Settings.Default.apiKey;

            //Parse Arguments
            ParseArguments();

            //Init logger class
#if DEBUG
            _log.Initialize("TACS_", "", "./log", LogIntervalType.IT_PER_DAY, LogLevel.D, true, true, true);
#else
            _log.Initialize("TACS_", "", "./log", LogIntervalType.IT_PER_DAY, LogLevel.E, true, true, true);
#endif

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
                Console.WriteLine("  --mumble\tSet the mumble file to use");
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
            if(CommandLine["mumble"] != null)
            {
                _mumbleFile = CommandLine["mumble"];
            }    
        }

        /// <summary>
        /// Checks for APIKey user setting on Appdata.
        /// </summary>
        /// <returns>0 if the APIKey setting was found or user succesfully stored it using the dialog.</returns>
        private int CheckForAPIKey()
        {
            if (apiKey.Length == 0)
            {
                return ShowKeyDialog();
            }
            return 0;
        }

        private int ShowKeyDialog()
        {
            var keyWindow = new APIKey();
            keyWindow.Owner = this;
            bool? dialogResult = keyWindow.ShowDialog();

            if (dialogResult.HasValue && !dialogResult.Value)
            {
                // User canceled, stop everything
                return 1;
            }

            //Reload Key
            apiKey = Properties.Settings.Default.apiKey;
            return 0;
        }

        private void GameStateChanged(object sender, GameStateChangedArgs e)
        {
            _log.AddNotice($"Game State is: {e.GameState}");
            switch (e.GameState)
            {
                case GameState.NotRunning:
                    //Stop Mumble Watcher
                    mumble.Stop();
                    //Disconnect from the chat server
                    client.Dispose();
                    //Stop Discord
                    discord.Stop();
                    //Hide the UI
                    HideUI();
                    break;
                case GameState.Launcher:
                    //IDK?
                    //Stop the mumble update also I guess
                    mumble.Stop();
                    //Stop Discord
                    discord.Stop();
                    break;
                case GameState.InGame:
                    //Start Mumble Watcher
                    mumble.Start();
                    //Start Discord RPC
                    discord.Start();
                    //Connect to the chat server (if needed)
                    ConnectToServer();
                    //Show the UI
                    //ShowUI();
                    AttachToGame(game.GameProcess.MainWindowHandle);
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

            //Menu
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
            Show();
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
                _log.AddDebug($"RECV> {rawPacket}");
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
                        case PacketType.AUTH:
                            if ((bool)packet.valid)
                            {
                                clientState = State.Authed;
                            } else {
                                clientState = State.BadAuth;
                                WriteToChat("Auth failed!");
                                WriteToChat((string)packet.reason);
                                client.Dispose();
                            }

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
            clientState = State.Connected;

            WriteToChat("Server connected");
            //Send Connect Packet
            client.Send(new Connect(apiKey).Send());
        }

        private void ServerDisconnected(object sender, EventArgs e)
        {
            //Dont reconnect if the game is closed or bad auth
            if (game.GameState == GameState.NotRunning || clientState == State.BadAuth)
                return;
            
            clientState = State.Disconnected;
            WriteToChat("Server disconnected, Retrying in 5 seconds.");
            new Thread(TryReconnect).Start();
        }

        private void TryReconnect()
        {
            while (!client.IsConnected)
            {
                Thread.Sleep(5000);
                ConnectToServer(true);
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
            if (e.Key == Key.Return && message.Text.Length != 0)
            {
                switch (message.Text)
                {
                    case "/setkey":
                        ShowKeyDialog();
                        break;
                    case "/togglestayopen":
                        stayOpenOnMap = !stayOpenOnMap;
                        Properties.Settings.Default.stayOpenOnMap = stayOpenOnMap;
                        Properties.Settings.Default.Save();
                        WriteToChat($"StayOpenOnMap changed to {stayOpenOnMap}");
                        break;
                    case "/map":
                        debugMap();
                        break;
                    default:
                        if (client.IsConnected)
                        {
                            //send packet as nomral
                            client.Send(new Message(message.Text).Send());
                        } else
                        {
                            //Alert the user they are not connected
                            WriteToChat("You are not currently connected to the chat server.");
                        }
                        break;
                }
                message.Text = "";
            }
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
            if (_isExit)
            {
                GlobalKeyboardHook.Instance.UnHook(hookId);
                mumble.Stop();
                game.Stop();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Check For API key
            int apiCheckResult = CheckForAPIKey();
            if (apiCheckResult != 0)
            {
                Close();
                Application.Current.Shutdown(0);
                return;
            }

            //Clear the chat log
            ChatBox.Document.Blocks.Clear();

            //Start Hidden
            HideUI();

            //Start watching for game
            game = new Game();
            game.GameStateChanged += GameStateChanged;
            game.Start();

            //Create Discord RPC
            discord = new DiscordRichPresence();

            //Setup Mumble Updater
            mumble = new Mumble(_mumbleFile);
            mumble.MapStatusChanged += IsMapShowing;
            //mumble.GameActiveStatusChanged += OnGameActiveChange;
            mumble.MumbleUpdated += OnMumbleUpdated;
            mumble.HasSelectedCharacter += OnFirstCharacteSelected;

            //Hook Shift+Enter hotkey
            hookId = GlobalKeyboardHook.Instance.Hook(new List<Key> { Key.RightShift, Key.Enter }, FocusChat, out string errorMessage);

        }

        private void FocusChat()
        {
            this.Activate();
            this.Focus();
            message.Focus();
        }

        private void ConnectToServer(bool reconnect = false)
        {
            //Do nothing if we are already connected
            if (!(client is null) && client.IsConnected) return;

            //Dispose of old connection if reconnecting
            if (reconnect) client.Dispose();

            _log.AddInfo($"Connecting to {serverAddr}:{serverPort}");
            client = new TcpClient(serverAddr, serverPort, false, null, null);
            client.Connected += ServerConnected;
            client.Disconnected += ServerDisconnected;
            client.DataReceived += MessageReceived;
            try
            {
                client.Connect();
            }
            catch (Exception)
            {
                WriteToChat("Can not connect to server, retrying in 5 seconds");
                if(!reconnect) new Thread(TryReconnect).Start();
            }
        }

        public void ShowUI()
        {
            Dispatcher.Invoke(() =>
            {
                Show();
            });
        }

        public void HideUI()
        {
            Dispatcher.Invoke(() =>
            {
                Hide();
            });
        }

        private void IsMapShowing(object sender, MapStatusChangedArgs e)
        {
            //Dont do anything it map is set to stay open
            if (stayOpenOnMap) return;

            if (e.IsMapOpen)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }

        private void OnGameActiveChange(object sender, GameWindowActiveArgs e)
        {
            if (e.IsGW2Active)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = true;
                });
            } else
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                });
            }
        }

        private void OnMumbleUpdated(object sender, MumbleUpdatedArgs mumble)
        {
            //If connected to the server and Authed
            if (clientState == State.Authed && !(client is null) && client.IsConnected)
            {
                //update Discord (this should be optimised)
                discord.Update(mumble.MumbleData.CharacterName, mumble.MumbleData.MapId);

                //Send update packet
                client.Send(new Update(mumble.MumbleData).Send());
            }
        }

        private void OnFirstCharacteSelected(Object sender, EventArgs e)
        {
            ShowUI();
        }

        private void AttachToGame(IntPtr hWnd)
        {
            Dispatcher.Invoke(() =>
            {
                new WindowInteropHelper(this) { Owner = hWnd };
                ShowInTaskbar = false;
            });
        }

        private void DetachFromGame()
        {
            Dispatcher.Invoke(() =>
            {
                ShowInTaskbar = true;
            });
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //Topmost = true;
        }

        private void debugMap()
        {
            using Gw2Client apiclient = new Gw2Client();
            apiclient.Mumble.Update();
            var mapData = apiclient.WebApi.V2.Maps.GetAsync(apiclient.Mumble.MapId);
            mapData.Wait();
            WriteToChat($"{mapData.Result.RegionName}");
            WriteToChat($"\"{hashRectangle(mapData.Result.ContinentRect)}\": , //{mapData.Result.Name}");
        }

        private int hashRectangle (Gw2Sharp.WebApi.V2.Models.Rectangle Rect)
        {
            return $"{Rect.TopLeft}.{Rect.TopRight}.{Rect.BottomLeft}.{Rect.BottomRight}".GetHashCode();
        }
    }

    enum State
    {
        Disconnected,
        Connected,
        Authed,
        BadAuth
    }
}
