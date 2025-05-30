using System;

namespace Contracts.Lifecycle
{
    public sealed record ServerState(
        ServerInstanceId ServerInstanceId,
        ServerStatus Status,
        int? Pid,
        DateTime? StartedUtc,
        DateTime? StoppedUtc,
        int CrashCountLastWindow);
}
