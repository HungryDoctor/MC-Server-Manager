using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Configuration
{
    public interface IConfiguration
    {
        Task<GlobalSettings> GetAsync(CancellationToken ct = default);
        Task SaveAsync(GlobalSettings settings, CancellationToken ct = default);
    }
}
