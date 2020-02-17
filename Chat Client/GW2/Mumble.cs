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
            var connection = new Connection();
            var GW2client = new Gw2Client(connection);
            MumbleData = GW2client.Mumble;
        }

        public void HookGame()
        {
            //IsAvailable will always be false untill you call Update
            MumbleData.Update();

            while (MumbleData.IsAvailable == false)
            {
                Console.WriteLine("MumbleAPI is not ready (at first character selection or first map load)");
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
                Thread.Sleep(100);
            }
        }
    }
}
