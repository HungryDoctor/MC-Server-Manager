using Contracts.Lifecycle;
using System;

namespace Contracts.Notification
{
    public sealed record NotificationFilter(
        NotifyLevel? MinimumLevel,
        DateTimeOffset? SinceUtc,
        ServerInstanceId? OnlyServer);
}
