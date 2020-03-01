using System;
using System.Threading;
using DLG.ToolBox.Log;
using Gw2Sharp;
using Gw2Sharp.Mumble;

namespace Chat_Client
{
    public class Mumble
    {
        private static readonly Logger _log = Logger.getInstance();
        private IGw2MumbleClient _mumbleClient;
        private bool _requestStop;
        private Thread _mumbleRefresher;
        private bool _lastMapState;

        public event EventHandler<MapStatusChangedArgs> MapStatusChanged;
        public event EventHandler<MumbleUpdatedArgs> MumbleUpdated;

        public Mumble()
        {
            _mumbleClient = new Gw2Client().Mumble;
            _requestStop = false;
        }

        public void Start()
        {
            _log.AddDebug("Starting Mumble Refresh Thread");
            _requestStop = false;
            _mumbleRefresher = new Thread(MumbleRefreshLoop);
            _mumbleRefresher.Start();
        }

        public void Stop()
        {
            _log.AddDebug("Stopping Mumble Refresh Thread");
            _requestStop = true;
        }

        private void MumbleRefreshLoop()
        {
            bool firstUpdate = true;
            int lastTick = -1;
            
            while (!_requestStop)
            {
                bool shouldRun = true;
                _mumbleClient.Update();

                //Check if Mumble is ready
                if (!_mumbleClient.IsAvailable)
                    shouldRun = false;

                //Check if player is in a map
                if (_mumbleClient.MapId == 0)
                    shouldRun = false;

                //Dont update if tick hasnt changed
                if (_mumbleClient.Tick == lastTick)
                    shouldRun = false;

                //React to new mumble info
                if (shouldRun)
                {
                    if (_lastMapState != _mumbleClient.IsMapOpen)
                    {
                        _lastMapState = _mumbleClient.IsMapOpen;
                        MapStatusChanged?.Invoke(this, new MapStatusChangedArgs(_lastMapState));
                    }
                    //Only update the server every 500 ticks
                    if (firstUpdate || (_mumbleClient.Tick % 500) == 0)
                    {
                        if (firstUpdate) firstUpdate = false;

                        _log.AddDebug("Sending Update");
                        MumbleUpdated?.Invoke(this, new MumbleUpdatedArgs(_mumbleClient));
                    }

                    //Update lastTick
                    lastTick = _mumbleClient.Tick;
                }
                //Take a nap
                Thread.Sleep(1000 / 60);
            }
        }
    }
}
