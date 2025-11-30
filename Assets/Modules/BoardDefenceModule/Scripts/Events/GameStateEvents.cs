namespace BoardDefence.Events
{
    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState NewState;
    }
}
