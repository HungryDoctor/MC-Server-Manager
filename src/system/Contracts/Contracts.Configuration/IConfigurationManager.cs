using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Configuration
{
    public interface IConfigurationManager
    {
        Task<GlobalSettings> GetAsync(CancellationToken ct = default);
        Task SaveAsync(GlobalSettings settings, CancellationToken ct = default);
    }
}
