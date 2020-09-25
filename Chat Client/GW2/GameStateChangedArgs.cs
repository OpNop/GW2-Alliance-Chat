using System;

namespace Chat_Client
{
    public class GameStateChangedArgs : EventArgs
    {
        public GameState GameState { get; private set; }
        public IntPtr? GameHandle { get; private set; }
        internal GameStateChangedArgs(GameState gameState, IntPtr? gameHandle = null)
        {
            GameState = gameState;
            GameHandle = gameHandle;
        }

    }
}