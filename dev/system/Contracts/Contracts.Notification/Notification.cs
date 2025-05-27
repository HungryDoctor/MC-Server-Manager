using System;

namespace Contracts.Notification
{
    public sealed record Notification(
        NotificationId Id,
        DateTimeOffset CreatedUtc,
        NotifyLevel Level,
        NotificationCode Code,
        string Message,
        object? Extra);
}
