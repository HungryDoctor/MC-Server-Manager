using Contracts.Lifecycle;
using Infrastructure.OS.Processes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Services.Lifecycle.ServerProcess
{
    public interface IServerProcessHost : IProcessHost, INotifyPropertyChanged
    {
        IAsyncEnumerable<ConsoleOutput> GetOutputBufferAsync(CancellationToken ct = default);
    }
}
