using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Lifecycle
{
    public interface IServerProcess
    {
        Task<ServerState> StartAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default);
        Task<ServerState> StopAsync(ServerInstanceId serverInstanceId, bool graceful, CancellationToken ct = default);
        Task<ServerState> RestartAsync(ServerInstanceId serverInstanceId, bool graceful, CancellationToken ct = default);
        IAsyncEnumerable<string> AttachConsoleAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default);
        Task SendCommandAsync(ServerInstanceId serverInstanceId, string command, CancellationToken ct = default);

    }
}
