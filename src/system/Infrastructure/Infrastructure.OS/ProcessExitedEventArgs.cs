namespace Infrastructure.OS
{
    /// <summary>
    /// On Linux, it is possible to get ExitCode only when the app has started the process
    /// See https://github.com/dotnet/runtime/issues/95831
    /// We are trying to fallback to read from /proc/[pid]/stat, but it is unreliable solution
    /// </summary>
    /// <param name="ExitCode">Application exit code</param>
    public record ProcessExitedEventArgs(int? ExitCode);
}
