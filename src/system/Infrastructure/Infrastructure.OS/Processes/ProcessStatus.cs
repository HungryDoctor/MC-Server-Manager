namespace Infrastructure.OS.Processes
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
