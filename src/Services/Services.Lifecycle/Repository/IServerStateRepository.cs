using Contracts.Lifecycle;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Lifecycle.Repository
{
    public interface IServerStateRepository
    {
        Task<ServerInstanceId> CreateAsync(ServerState serverState, CancellationToken ct = default);
        Task<ServerState?> GetAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default);
        Task<IReadOnlySet<ServerState>> GetAsync(CancellationToken ct = default);
        Task UpdateAsync(ServerState serverState, CancellationToken ct = default);
        Task DeleteAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default);
    }
}
