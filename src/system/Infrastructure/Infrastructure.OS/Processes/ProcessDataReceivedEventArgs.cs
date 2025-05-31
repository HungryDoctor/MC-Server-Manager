namespace Infrastructure.OS.Processes
{
    public record ProcessDataReceivedEventArgs(int? Pid, string? Data);
}
