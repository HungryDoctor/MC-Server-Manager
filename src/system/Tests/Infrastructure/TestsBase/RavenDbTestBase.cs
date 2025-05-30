using Raven.Client.Documents;
using Raven.Embedded;
using System.Threading.Tasks;

namespace TestsBase
{
    public abstract class RavenDbTestBase
    {
        protected IDocumentStore m_documentStore;


        [Before(HookType.TestSession)]
        public static void InitializeRavenDb()
        {
            EmbeddedServer.Instance.StartServer();
        }

        [After(HookType.TestSession)]
        public static void StopRavenDb()
        {
            EmbeddedServer.Instance.Dispose();
        }

        [Before(HookType.Test)]
        public async Task InitializeDocumentStoreAsync()
        {
            m_documentStore = await EmbeddedServer.Instance.GetDocumentStoreAsync(Guid.NewGuid().ToString()).ConfigureAwait(false);
        }

        [After(HookType.Test)]
        public void CleanupDocumentStore()
        {
            m_documentStore.Dispose();
        }
    }
}
