using System;
using System.Threading;
using Chat_Client.utils.Log;
using Gw2Sharp;
using Gw2Sharp.Mumble;

namespace Chat_Client
{
    public class Mumble
    {
        private const string DEFAULT_MUMBLE_LINK_MAP_NAME = "MumbleLink";

        private static readonly Logger _log = Logger.getInstance();
        private readonly IGw2MumbleClient _mumbleClient;

        private Thread _mumbleRefresher;
        private string _mumbleLinkFile;
        private bool _requestStop;
        private bool _lastMapState;
        private bool _lastActiveState;

        public event EventHandler<MapStatusChangedArgs> MapStatusChanged;
        public event EventHandler<MumbleUpdatedArgs> MumbleUpdated;
        public event EventHandler<GameWindowActiveArgs> GameActiveStatusChanged;
        public event EventHandler HasSelectedCharacter;

        public Mumble(string mumbleFile = DEFAULT_MUMBLE_LINK_MAP_NAME)
        {
            _mumbleLinkFile = mumbleFile;
            _mumbleClient = new Gw2Client().Mumble[_mumbleLinkFile];
            _requestStop = false;
        }

        public void Start()
        {
            _log.AddInfo($"Starting Mumble Refresh Thread (Link: {_mumbleLinkFile})");
            _requestStop = false;
            _mumbleRefresher = new Thread(MumbleRefreshLoop);
            _mumbleRefresher.Start();
        }

        public void Stop()
        {
            _log.AddInfo("Stopping Mumble Refresh Thread");
            _requestStop = true;
        }

        private void MumbleRefreshLoop()
        {
            //Update the mumble to detect stale data
            _mumbleClient.Update();

            bool firstUpdate = true;
            int? lastTick = null;
            lastTick ??= _mumbleClient?.Tick ?? 0;
            
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
                    //Trigger the ShowUI when a character is selected
                    if (firstUpdate)
                        HasSelectedCharacter?.Invoke(this, EventArgs.Empty);

                    if (_lastMapState != _mumbleClient.IsMapOpen)
                    {
                        _lastMapState = _mumbleClient.IsMapOpen;
                        MapStatusChanged?.Invoke(this, new MapStatusChangedArgs(_lastMapState));
                    }

                    if(_lastActiveState != _mumbleClient.DoesGameHaveFocus)
                    {
                        _lastActiveState = _mumbleClient.DoesGameHaveFocus;
                        GameActiveStatusChanged?.Invoke(this, new GameWindowActiveArgs(_lastActiveState));
                    }

                    //Only update the server every 500 ticks
                    if (firstUpdate || (_mumbleClient.Tick % 100) == 0)
                    {
                        if (firstUpdate) firstUpdate = false;

                        _log.AddDebug($"Sending Update (Tick = {_mumbleClient.Tick})");
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