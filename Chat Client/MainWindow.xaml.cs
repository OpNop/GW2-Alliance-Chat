using Chat_Client.Packets;
using Chat_Client.Utils;
using Chat_Client.Utils.Log;
using Gw2Sharp;
using Newtonsoft.Json;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

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
        private bool debug = true;
#else
        private string serverAddr = "chat.jumpsfor.science";
        private int serverPort = 8888;
        private bool debug = false;
#endif

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SwitchToThisWindow(IntPtr hWnd, Boolean fAltTab);
        private const int WM_EXITSIZEMOVE = 0x232;

        private Settings Settings = new Settings();

        private TcpClient client;
        private State clientState = State.Disconnected;
        private bool _autoScroll = true;
        //private bool _debugMode = false;
        private string _mumbleFile = "MumbleLink";

        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        public Game game;
        public Mumble mumble;
        public DiscordRichPresence discord;
        private static readonly Logger _log = Logger.getInstance();
        private readonly string _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Guild Wars 2\addons\TACS\Logs");
        private SoundPlayer notifyPlayer;

        //Session Settings
        private int hookId;
        private bool IsClosed;

        public MainWindow()
        {
            //Load Settings
            Settings = Settings.Load();
            DataContext = Settings;

            InitializeComponent();

            //Init logger class
            _log.Initialize("TACS_", "", _logPath, LogIntervalType.IT_PER_DAY, LogLevel.E, true, false, true);

            //Parse Arguments
            ParseArguments();

            //Setup Tray Icon
            SetupNotifyIcon();
            
            //Setup notification sound
            notifyPlayer = new SoundPlayer(Application.GetResourceStream(new Uri("pack://application:,,,/Assets/notify.wav")).Stream);
            notifyPlayer.Load();
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

            // --debug
            if (CommandLine["debug"] != null)
            {
                SetDebug(true);
                debug = true;
                _log.ChangeLevel(LogLevel.D);
                Console.SetOut(new ConsoleWritter(ChatBox));
                _log.AddWarning("Starting in Debug Mode");
            }

            // --server
            if (CommandLine["server"] != null)
            {
                serverAddr = CommandLine["server"];
                _log.AddInfo($"Using Server IP: {serverAddr}");
            }

            // --port
            if (CommandLine["port"] != null)
            {
                serverPort = int.Parse(CommandLine["port"]);
                _log.AddInfo($"Using Server Port: {serverPort}");
            }

            // --mumble
            if (CommandLine["mumble"] != null)
            {
                _mumbleFile = CommandLine["mumble"];
            }
        }

        public void SetDebug(bool enableDebug)
        {
            if (enableDebug)
            {
                // Redirect console to chat window
                Console.SetOut(new ConsoleWritter(ChatBox));
                _log.ChangeLevel(LogLevel.D);
                _log.AddInfo("Turining on debug.");
                debug = true;
            }
            else
            {
                // Reset console output
                _log.AddInfo("Turining off debug.");
                _log.ChangeLevel(LogLevel.E);
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                debug = false;
            }
        }

        /// <summary>
        /// Checks for APIKey user setting on Appdata.
        /// </summary>
        /// <returns>0 if the APIKey setting was found or user succesfully stored it using the dialog.</returns>
        private int CheckForAPIKey()
        {
            if (string.IsNullOrEmpty(Settings.APIKey))
            {
                return ShowKeyDialog();
            }
            return 0;
        }

        private int ShowKeyDialog()
        {
            var keyWindow = new APIKey(Settings);
            keyWindow.Owner = this;
            bool? dialogResult = keyWindow.ShowDialog();

            if (dialogResult.HasValue && !dialogResult.Value)
            {
                // User canceled, stop everything
                return 1;
            }

            return 0;
        }

        private void SettingsUpdate(object sender, PropertyChangedEventArgs args)
        {
            //watch non databound setting
            switch (args.PropertyName)
            {
                case "EnableDiscord":
                    UpdateDiscord();
                    break;
            }
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
                    //Start Discord RPC (if enabled)
                    if (Settings.EnableDiscord) discord.Start(game.GameProcess.StartTime);
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
            _notifyIcon.Text = $"Tiny Alliance Chat Service (TACS)\nversion {getVersionText()}";

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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Call Dispose to remove the icon out of notification area of Taskbar.
            _notifyIcon.Dispose();
        }

        private void ShowMainWindow()
        {
            IsClosed = false;

            if (game.GameState == GameState.InGame)
                Show();
            else
                MessageBox.Show("To use TACS, please launch the game and select a character.", "Nothing to see here", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
                            WriteToChat($"{packet.user} is online", Brushes.GreenYellow);
                            break;
                        case PacketType.LEAVE:
                            //Handle LEAVE message
                            WriteToChat($"{packet.user} is offline", Brushes.GreenYellow);
                            break;
                        case PacketType.MESSAGE:
                            WriteToChat(DateTime.Now.ToString("h:mm tt"), (string)packet.name, (string)packet.message);
                            await PlaySound();
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
                            }
                            else
                            {
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

        private async Task PlaySound()
        {
            if (this.Visibility != Visibility.Visible)
            {
                notifyPlayer.Play();
            }
        }

        async void ServerConnected(object sender, EventArgs e)
        {
            clientState = State.Connected;

            WriteToChat("Server connected");
            //Send Connect Packet
            client.Send(new Connect(Settings.APIKey).Send());
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
                if (Settings.ShowTimestamp) ChatBox.AppendText($"[{time}] ", Brushes.Gray);
                ChatBox.AppendText($"{from}: ", Brushes.DarkTurquoise);
                ChatBox.AppendText(message, Brushes.PowderBlue);
                ChatBox.AppendText(Environment.NewLine);
                ChatBox.ScrollToEnd();
            });
        }

        private void WriteToChat(string message, Brush color = null)
        {
            if (color == null)
                color = Brushes.DarkGray;

            ChatBox.Dispatcher.Invoke(() =>
            {
                if (Settings.ShowTimestamp)
                {
                    var time = DateTime.Now.ToString("h:mm tt");
                    ChatBox.AppendText($"[{time}] ", Brushes.Gray);
                }
                ChatBox.AppendText(message, color);
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
            if (Keyboard.IsKeyDown(Key.RightShift) && e.Key == Key.Return)
                return;

            if (e.Key == Key.Return)
            {
                if (message.Text.Length != 0)
                {
                    switch (message.Text)
                    {
                        case "/setkey":
                            ShowKeyDialog();
                            break;
                        case "/togglestayopen":
                            Settings.ShowOnMap = !Settings.ShowOnMap;
                            WriteToChat($"StayOpenOnMap changed to {Settings.ShowOnMap}");
                            break;
                        case "//debug":
                            SetDebug(!debug);
                            break;
                        case "/map":
                            debugMap();
                            break;
                        case "/version":
                            WriteToChat($"You are running TACS version {getVersionText()}.");
                            break;
                        default:
                            if (client.IsConnected)
                            {
                                //send packet as nomral
                                client.Send(new Message(message.Text).Send());
                            }
                            else
                            {
                                //Alert the user they are not connected
                                WriteToChat("You are not currently connected to the chat server.");
                            }
                            break;
                    }
                    message.Text = "";
                }
                //set focus back to the game
                SwitchToThisWindow(game.GameProcess.MainWindowHandle, true);
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
            Top = Settings.ChatTop;
            Left = Settings.ChatLeft;
            Height = Settings.ChatHeight;
            Width = Settings.ChatWidth;

            base.OnSourceInitialized(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                Hide();
                IsClosed = true;
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

            //Hook for resizing
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

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

            Settings.PropertyChanged += SettingsUpdate;

        }

        private void FocusChat()
        {
            Show();
            Activate();
            Focus();
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
                if (!reconnect) new Thread(TryReconnect).Start();
            }
        }

        private void UpdateDiscord()
        {
            if (Settings.EnableDiscord)
                discord.Start(game.GameProcess.StartTime);
            else
                discord.Stop();
        }

        public void ShowUI()
        {
            if (IsClosed) return; //Closed by user
            
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
            if (Settings.ShowOnMap) return;

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
            }
            else
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
                if (Settings.EnableDiscord) discord.Update(mumble.MumbleData.CharacterName, mumble.MumbleData.MapId);

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

        private void debugMap()
        {
            using Gw2Client apiclient = new Gw2Client();
            apiclient.Mumble.Update();
            var mapData = apiclient.WebApi.V2.Maps.GetAsync(apiclient.Mumble.MapId);
            mapData.Wait();
            WriteToChat($"{mapData.Result.RegionName}");
            WriteToChat($"\"{hashRectangle(mapData.Result.ContinentRect)}\": , //{mapData.Result.Name}");
        }

        private int hashRectangle(Gw2Sharp.WebApi.V2.Models.Rectangle Rect)
        {
            return $"{Rect.TopLeft}.{Rect.TopRight}.{Rect.BottomLeft}.{Rect.BottomRight}".GetHashCode();
        }

        private void btnSettings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SettingsPopup.IsOpen = true;
            SettingsPopup.Focus();
        }

        private void ChangeKey_Click(object sender, RoutedEventArgs e)
        {
            ShowKeyDialog();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_EXITSIZEMOVE)
            {
                //Save window location and size
                Settings.ChatTop = this.Top;
                Settings.ChatLeft = this.Left;
                Settings.ChatHeight = this.Height;
                Settings.ChatWidth = this.Width;
            }

            return IntPtr.Zero;
        }

        private string getVersionText()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
    }

    public class ConsoleWritter : TextWriter
    {
        private readonly RichTextBox textbox;
        public ConsoleWritter(RichTextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Dispatcher.Invoke(() =>
            {
                textbox.AppendText(value.ToString());
            });
        }

        public override void Write(string value)
        {
            textbox.Dispatcher.Invoke(() =>
            {
                var logType = value.Substring(0, 2);

                Brush color = logType switch
                {
                    "E " => Brushes.Red,
                    "W " => Brushes.Yellow,
                    "I " => Brushes.White,
                    "N " => Brushes.Green,
                    "D " => Brushes.Cyan,
                    _ => Brushes.Gray,
                };
                textbox.AppendText(value, color);
            });
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
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
