using Contracts.Configuration;
using System;

namespace Services.Configuration.Defaults
{
    internal static class GlobalSettingsDefaults
    {
        public const string DefaultBackupRoot = "/backups";
        public const string DefaultServersRoot = "/instances";
        public const string DefaultJdksRoot = "/jdks";

        public const uint LogTailKBDefault = 1024;
        public static readonly TurnOffMessage[] TurnOffMessages = Array.Empty<TurnOffMessage>();

        public const uint CrashLoopCount = 3;
        public static readonly TimeSpan CrashLoopWindow = TimeSpan.FromMinutes(10);

        public const string JdkScanCron = "0 0 3 1 * ?";                // At 03:00 on the 1st of every month
        public const string ModLoaderScanCron = "0 0 3 ? * SUN";        // At 03:00 every Sunday


        public static readonly GlobalSettings Instance = new GlobalSettings(
            DefaultBackupRoot,
            DefaultServersRoot,
            DefaultJdksRoot,
            LogTailKBDefault,
            TurnOffMessages,
            CrashLoopCount,
            CrashLoopWindow,
            JdkScanCron,
            ModLoaderScanCron);
    }
}
