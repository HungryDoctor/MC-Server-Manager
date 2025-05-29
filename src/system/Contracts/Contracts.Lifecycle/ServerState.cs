using System;

namespace Contracts.Lifecycle
{
    public sealed record ServerState(
        ServerInstanceId Id,
        ServerStatus Status,
        int? Pid,
        DateTimeOffset? StartedUtc,
        DateTimeOffset? StoppedUtc,
        int CrashCountLastWindow);
}
