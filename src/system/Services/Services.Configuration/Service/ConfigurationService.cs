using Contracts.Configuration;
using Services.Configuration.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Configuration.Service
{
    public sealed class ConfigurationService : IConfiguration
    {
        private readonly IConfigurationRepository m_repository;


        public ConfigurationService(IConfigurationRepository repo)
        {
            m_repository = repo;
        }


        public Task<GlobalSettings> GetAsync(CancellationToken ct = default) => m_repository.GetAsync(ct);
        public Task SaveAsync(GlobalSettings settings, CancellationToken ct = default) => m_repository.UpdateAsync(settings, ct);
    }
}
