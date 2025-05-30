using System;

namespace Contracts.Lifecycle
{
    public sealed record ServerState(
        ServerInstanceId ServerInstanceId,
        ServerStatus Status,
        int? Pid,
        DateTimeOffset? StartedUtc,
        DateTimeOffset? StoppedUtc,
        int CrashCountLastWindow);
}
