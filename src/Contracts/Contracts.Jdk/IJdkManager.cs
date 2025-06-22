using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Jdk
{
    public interface IJdkManager
    {
        Task<JdkInstanceId> InstallAsync(string vendor, string version, CancellationToken ct = default);
        Task<JdkInstanceId> ImportAsync(string absolutePath, CancellationToken ct = default);
        Task ReinstallAsync(JdkInstanceId id, CancellationToken ct = default);
        Task MoveAsync(JdkInstanceId id, string newPath, CancellationToken ct = default);
        Task DeleteAsync(JdkInstanceId id, CancellationToken ct = default);

        Task<IReadOnlySet<JdkInstanceId>> ListAsync(CancellationToken ct = default);
        Task<JdkInfo?> GetAsync(JdkInstanceId id, CancellationToken ct = default);

        Task<JdkUpdateStatus> GetUpdateStatusAsync(JdkInstanceId id, CancellationToken ct = default);
    }
}
