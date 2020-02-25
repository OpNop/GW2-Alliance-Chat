using System;

namespace Chat_Client
{
    public class MapStatusChangedArgs : EventArgs
    {
        public bool IsMapOpen { get; private set; }
        internal MapStatusChangedArgs(bool IsMapOpen)
        {
            this.IsMapOpen = IsMapOpen;
        }
    }
}