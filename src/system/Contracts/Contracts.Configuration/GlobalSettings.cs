namespace Contracts.Configuration
{
    public sealed record GlobalSettings(
        string DefaultBackupRoot,
        string DefaultServersRoot,
        string DefaultJdksRoot,
        int LogTailKBDefault,
        IReadOnlyList<TurnOffMessage> TurnOffMessages,
        int CrashLoopCount,
        TimeSpan CrashLoopWindow,
        string? JdkScanCron,
        string? ModLoaderScanCron);
}
