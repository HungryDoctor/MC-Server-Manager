using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Notification
{
    public interface INotificationSink
    {
        Task PublishAsync(Notification note, CancellationToken ct = default);
        IAsyncEnumerable<Notification> StreamAsync(NotificationFilter? filter, CancellationToken ct = default);
    }
}
