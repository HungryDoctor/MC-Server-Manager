using Infrastructure.OS.Processes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Services.Lifecycle
{
    public interface IServerProcessHost : IProcessHost, INotifyPropertyChanged
    {
        IAsyncEnumerable<string> GetOutputBufferAsync(CancellationToken ct = default);
    }
}
