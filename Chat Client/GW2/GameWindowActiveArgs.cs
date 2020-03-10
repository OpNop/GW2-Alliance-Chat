using System;

namespace Chat_Client
{
    public class GameWindowActiveArgs : EventArgs
    {
        public bool IsGW2Active { get; private set; }
        internal GameWindowActiveArgs(bool IsGW2Active)
        {
            this.IsGW2Active = IsGW2Active;
        }
    }
}