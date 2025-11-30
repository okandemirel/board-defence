namespace BoardDefence.Events
{
    public struct StartGameSignal { }

    public struct StartLevelSignal
    {
        public int LevelIndex;
    }

    public struct RestartLevelSignal { }

    public struct ReturnToMenuSignal { }

    public struct NextLevelSignal { }

    public struct CleanupLevelSignal { }

    public struct SelectDefenceSignal
    {
        public int DefenceIndex;
    }
}
