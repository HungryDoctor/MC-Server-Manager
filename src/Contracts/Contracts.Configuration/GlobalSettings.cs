using System;
using System.Collections.Generic;
using System.Linq;

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
        string ModLoaderScanCron) : IEquatable<GlobalSettings>
    {
        public bool Equals(GlobalSettings? other) =>
            other is not null &&
            DefaultBackupRoot == other.DefaultBackupRoot &&
            DefaultServersRoot == other.DefaultServersRoot &&
            DefaultJdksRoot == other.DefaultJdksRoot &&
            LogTailKBDefault == other.LogTailKBDefault &&
            CrashLoopCount == other.CrashLoopCount &&
            CrashLoopWindow == other.CrashLoopWindow &&
            JdkScanCron == other.JdkScanCron &&
            ModLoaderScanCron == other.ModLoaderScanCron &&
            TurnOffMessages.SequenceEqual(other.TurnOffMessages);

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(DefaultBackupRoot);
            hashCode.Add(DefaultServersRoot);
            hashCode.Add(DefaultJdksRoot);
            hashCode.Add(LogTailKBDefault);
            hashCode.Add(CrashLoopCount);
            hashCode.Add(CrashLoopWindow);
            hashCode.Add(JdkScanCron);
            hashCode.Add(ModLoaderScanCron);

            foreach (TurnOffMessage msg in TurnOffMessages)
            {
                hashCode.Add(msg);
            }

            return hashCode.ToHashCode();
        }
    }
}
