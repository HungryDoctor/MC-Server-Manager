using Contracts.Lifecycle;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Lifecycle.Repository
{
    public interface IServerProcessRepository
    {
        Task<ServerState> LoadAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default);
        Task<IReadOnlySet<ServerState>> ListAsync(CancellationToken ct = default);
        Task<ServerInstanceId> CreateAsync(ServerState serverState, CancellationToken ct = default);
        Task SaveAsync(ServerState serverState, CancellationToken ct = default);
    }
}
