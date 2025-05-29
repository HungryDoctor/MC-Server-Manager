using Contracts.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Configuration.Repository
{
    internal interface IConfigurationRepository
    {
        Task<GlobalSettings> GetAsync(CancellationToken ct = default);
        Task SaveAsync(GlobalSettings settings, CancellationToken ct = default);
    }
}
