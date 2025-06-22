namespace Contracts.Notification
{
    public enum NotificationCode
    {
        Custom,

        ServerStarted,
        ServerStopped,
        ServerCrashed,
        CrashLoopDetected,

        BackupSucceeded,
        BackupFailed,

        JdkUpdateAvailable,
        ModLoaderUpdateAvailable,
    }
}
