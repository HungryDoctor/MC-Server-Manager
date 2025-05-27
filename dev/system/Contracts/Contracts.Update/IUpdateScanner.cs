using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Update
{
    public interface IUpdateScanner
    {
        Task<int> GetAvailableJdkUpdatesAsync(CancellationToken ct = default);
        Task<int> GetAvailableModLoaderUpdatesAsync(CancellationToken ct = default);
    }
}
