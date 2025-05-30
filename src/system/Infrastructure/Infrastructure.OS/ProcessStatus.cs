namespace Infrastructure.OS
{
    public enum ProcessStatus : byte
    {
        NotStarted,
        Starting,
        Running,
        Exited,
        FailedToStart
    }
}
