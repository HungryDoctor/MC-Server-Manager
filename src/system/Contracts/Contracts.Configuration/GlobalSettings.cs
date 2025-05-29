using System;
using System.Collections.Generic;

namespace Contracts.Configuration
{
    public sealed record GlobalSettings(
        string DefaultBackupRoot,
        string DefaultServersRoot,
        string DefaultJdksRoot,
        uint LogTailKBDefault,
        IReadOnlyList<TurnOffMessage> TurnOffMessages,
        uint CrashLoopCount,
        TimeSpan CrashLoopWindow,
        string JdkScanCron,
        string ModLoaderScanCron);
}
