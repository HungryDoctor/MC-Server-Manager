using Contracts.Configuration;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Configuration.Repository
{
    public class RavenDbConfigurationRepository : IConfigurationRepository
    {
        private const string c_globalSettingsCollection = "global_settings";
        private readonly IDocumentStore m_documentStore;


        public RavenDbConfigurationRepository(IDocumentStore documentStore)
        {
            m_documentStore = documentStore;
        }


        public async Task<GlobalSettings?> GetAsync(CancellationToken ct = default)
        {
            using (IAsyncDocumentSession session = m_documentStore.OpenAsyncSession())
            {
                GlobalSettings? settings = await session.LoadAsync<GlobalSettings>(c_globalSettingsCollection, ct).ConfigureAwait(false);
                return settings;
            }
        }

        public async Task SaveAsync(GlobalSettings settings, CancellationToken ct = default)
        {
            using (IAsyncDocumentSession session = m_documentStore.OpenAsyncSession())
            {
                await session.StoreAsync(settings, c_globalSettingsCollection, ct).ConfigureAwait(false);
                await session.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }
    }
}
