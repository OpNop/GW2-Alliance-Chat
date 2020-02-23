using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gw2Sharp;
using Gw2Sharp.Mumble;

namespace Chat_Client
{
    public class Mumble
    {
        private IGw2MumbleClient _mumbleClient;
        private int tick = -1;
        public event EventHandler<MapStatusChangedArgs> MapStatusChanged;
        private bool LastMapState;

        public IGw2MumbleClient MumbleData {
            get => _mumbleClient;
            private set
            {
                if(value == null) { return; }

                //memory file is stale
                if( value.Tick == tick )
                {
                    _mumbleClient.Dispose();
                }
                else
                {
                    _mumbleClient = value;
                }
            }
        }

        public Mumble() { }

        public void Init()
        {
            
            var GW2client = new Gw2Client();
            MumbleData = GW2client.Mumble;
        }

        public void HookGame()
        {
            //IsAvailable will always be false untill you call Update
            MumbleData.Update();

            while (MumbleData.IsAvailable == false)
            {
                Console.WriteLine("MumbleAPI is not ready (at first character selection or first map load?)");
                MumbleData.Update();
                Thread.Sleep(1000);
            }

            tick = MumbleData.Tick;
        }

        public void UpdateMumble()
        {
            while (MumbleData.IsAvailable)
            {
                MumbleData.Update();
                if(LastMapState != MumbleData.IsMapOpen)
                {
                    LastMapState = MumbleData.IsMapOpen;
                    OnMapStatusChanged(new MapStatusChangedArgs() { IsMapOpen = LastMapState });
                }
                Thread.Sleep(100);
            }
        }

        protected virtual void OnMapStatusChanged(MapStatusChangedArgs e)
        {
            EventHandler<MapStatusChangedArgs> handler = MapStatusChanged;
            handler?.Invoke(this, e);
        }
    }

    public class MapStatusChangedArgs : EventArgs
    {
        public bool IsMapOpen { get; set; }
    }
}
