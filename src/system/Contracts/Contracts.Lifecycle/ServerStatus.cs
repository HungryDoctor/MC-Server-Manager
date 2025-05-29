namespace Contracts.Lifecycle
{
    public enum ServerStatus : byte
    {
        Unknown,
        Stopped,
        Stopping,
        Starting,
        Running,
        Crashed,
        CrashLoop
    }
}
