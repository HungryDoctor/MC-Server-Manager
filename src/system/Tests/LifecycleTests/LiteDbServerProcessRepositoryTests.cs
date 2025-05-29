using LiteDB;
using Services.Lifecycle.Repository;
using System.IO;

namespace LifecycleTests
{
    public class LiteDbServerProcessRepositoryTests
    {
        private MemoryStream m_dbMemoryStream;
        private LiteDatabase m_database;
        private LiteDbServerProcessRepository m_liteDbServerProcessRepository;


        [Before(HookType.Test)]
        public void InitializeTest()
        {
            m_dbMemoryStream = new MemoryStream();
            m_database = new LiteDatabase(m_dbMemoryStream);
            m_liteDbServerProcessRepository = new LiteDbServerProcessRepository(m_database);
        }

        [After(HookType.Test)]
        public void CleanupTest()
        {
            m_database.Dispose();
            m_dbMemoryStream.Dispose();
        }



    }
}
