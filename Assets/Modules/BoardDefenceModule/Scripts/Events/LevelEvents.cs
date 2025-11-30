namespace BoardDefence.Events
{
    public struct LevelStartedEvent
    {
        public int LevelIndex;
    }

    public struct LevelCompletedEvent
    {
        public int LevelIndex;
        public int FinalScore;
    }

    public struct GameOverEvent
    {
        public bool Victory;
        public int FinalScore;
    }

    public struct WaveStartedEvent
    {
        public int WaveIndex;
        public int TotalWaves;
    }

    public struct WaveCompletedEvent
    {
        public int WaveIndex;
    }
}
