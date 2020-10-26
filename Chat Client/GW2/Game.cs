using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Chat_Client.utils.Log;

namespace Chat_Client
{
    public class Game
    {
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        private const string GW2_PATCHWINDOW_NAME = "ArenaNet";
        private const string GW2_GAMEWINDOW_NAME = "ArenaNet_Dx_Window_Class";

        private static readonly Logger _log = Logger.getInstance();
        private Thread _gameWatcher;
        private bool _requestStop;
        private Process _gw2Process;
        private GameState _gameState;

        public event EventHandler<GameStateChangedArgs> GameStateChanged;

        public Process GameProcess
        {
            get => _gw2Process;
        }

        public GameState GameState
        {
            get => _gameState;
            private set
            {
                if (_gameState == value)
                    return;

                _gameState = value;

                GameStateChanged?.Invoke(this, new GameStateChangedArgs(GameState));
            }
        }

        public Game()
        {
            GameState = GameState.NotRunning;
            _requestStop = false;
        }

        public void Start()
        {
            _log.AddNotice("Starting Game Watcher Thread");
            _gameWatcher = new Thread(GameWatchLoop);
            _gameWatcher.SetApartmentState(ApartmentState.STA);
            _gameWatcher.Start();
        }

        public void Stop()
        {
            _log.AddNotice("Stopping Game Watcher Thread");
            _requestStop = true;
        }

        private void GameWatchLoop()
        {
            _log.AddInfo("Waiting for GW2 Process");

            while (!_requestStop)
            {
                if (_gw2Process == null)
                {
                    //Check if GuildWars is running
                    GetProcess();
                }
                else
                {
                    //Refresh data to catch window className change
                    _gw2Process.Refresh();

                    //Check what mode the game is in
                    var gameWindow = GetWindowClassName(_gw2Process.MainWindowHandle);
                    if (gameWindow == GW2_PATCHWINDOW_NAME)
                    {
                        GameState = GameState.Launcher;
                    }
                    else if (gameWindow == GW2_GAMEWINDOW_NAME)
                    {
                        //Game is in proper game
                        _requestStop = true;
                        GameState = GameState.InGame;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void GetProcess()
        {
            _log.AddDebug("Checking for GW2 Process");
            
            // Only checking for x64 client 
            Process[] Processes = Process.GetProcessesByName("Gw2-64");
            
            if (Processes.Length >= 1 )
            {
                _log.AddInfo($"Found {Processes.Length} Processes");
                _gw2Process = Processes[0];
                _gw2Process.EnableRaisingEvents = true;
                _gw2Process.Exited += GameExited;
            }
        }

        private void GameExited(object sender, EventArgs e)
        {
            _log.AddNotice("Game exited");
            GameState = GameState.NotRunning;
            _gw2Process = null;
            _requestStop = false;

            //Start the watcher again
            Start();
        }

        private string GetWindowClassName(IntPtr handle)
        {
            StringBuilder className = new StringBuilder(100);
            GetClassName(handle, className, className.Capacity);
            return className.ToString();
        }
    }

    public enum GameState
    {
        NotRunning,
        Launcher,
        InGame
    }
}