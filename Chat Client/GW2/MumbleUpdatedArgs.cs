using Gw2Sharp.Mumble;
using System;

namespace Chat_Client
{
    public class MumbleUpdatedArgs : EventArgs
    {
        public IGw2MumbleClient MumbleData {get; private set;}

        internal MumbleUpdatedArgs (IGw2MumbleClient MumbleData)
        {
            this.MumbleData = MumbleData;
        }
    }
}