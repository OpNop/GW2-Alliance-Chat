using System;

namespace Chat_Client
{
    public class GameStateChangedArgs : EventArgs
    {
        public GameState GameState { get; private set; }
        internal GameStateChangedArgs(GameState gameState)
        {
            GameState = gameState;
        }

    }
}