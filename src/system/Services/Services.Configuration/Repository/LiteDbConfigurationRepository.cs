using Contracts.Configuration;
using LiteDB;
using Services.Configuration.Defaults;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Configuration.Repository
{
    public class LiteDbConfigurationRepository : IConfigurationRepository
    {
        private const string c_globalSettingsCollection = "global_settings";
        private readonly ILiteDatabase m_db;


        public LiteDbConfigurationRepository(ILiteDatabase db)
        {
            m_db = db;
        }


        public Task<GlobalSettings> GetAsync(CancellationToken ct = default)
        {
            GlobalSettings? doc = m_db.GetCollection<GlobalSettings>(c_globalSettingsCollection).FindById(1);
            return Task.FromResult(doc ?? GlobalSettingsDefaults.Instance);
        }

        public Task SaveAsync(GlobalSettings settings, CancellationToken ct = default)
        {
            m_db.GetCollection<GlobalSettings>(c_globalSettingsCollection)
                .Upsert(1, settings);
            m_db.Checkpoint();

            return Task.CompletedTask;
        }
    }
}
