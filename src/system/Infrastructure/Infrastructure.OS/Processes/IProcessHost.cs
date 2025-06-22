using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OS.Processes
{
    public interface IProcessHost : IAsyncDisposable
    {
        ProcessStatus Status { get; }
        int ProcessId { get; }

        int Start();
        Task StopAsync(CancellationToken ct = default);
        Task StopAsync(TimeSpan waitForExit, CancellationToken ct = default);
        Task SendCommandAsync(string command, CancellationToken ct = default);
    }
}
