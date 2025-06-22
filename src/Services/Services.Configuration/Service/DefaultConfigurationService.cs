using Contracts.Configuration;
using Services.Configuration.Defaults;
using Services.Configuration.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Configuration.Service
{
    public sealed class DefaultConfigurationService : IConfigurationManager
    {
        private readonly IConfigurationRepository m_repository;


        public DefaultConfigurationService(IConfigurationRepository repo)
        {
            m_repository = repo;
        }


        public async Task<GlobalSettings> GetAsync(CancellationToken ct = default)
        {
            GlobalSettings? result = await m_repository.GetAsync(ct).ConfigureAwait(false);
            return result ?? GlobalSettingsDefaults.Instance;
        }
        public Task SaveAsync(GlobalSettings settings, CancellationToken ct = default) => m_repository.CreateOrUpdateAsync(settings, ct);
    }
}
