using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;
using Raven.Embedded;
using System;
using System.Threading.Tasks;

namespace TestsBase
{
    public abstract class RavenDbTestBase
    {
        protected IDocumentStore m_documentStore = null!;


        [Before(HookType.TestSession)]
        public static void InitializeRavenDb()
        {
            EmbeddedServer.Instance.StartServer();
        }

        [After(HookType.TestSession)]
        public static void StopRavenDb()
        {
            try
            {
                EmbeddedServer.Instance.Dispose();
            }
            catch
            {
                // Disposing
            }
        }

        [Before(HookType.Test)]
        public async Task InitializeDocumentStoreAsync()
        {
            m_documentStore = await EmbeddedServer.Instance.GetDocumentStoreAsync(Guid.NewGuid().ToString()).ConfigureAwait(false);
        }

        [After(HookType.Test)]
        public async Task CleanupDocumentStoreAsync()
        {
            try
            {
                await m_documentStore.Maintenance.Server.SendAsync(new DeleteDatabasesOperation(m_documentStore.Database, true)).ConfigureAwait(false);
                m_documentStore.Dispose();
            }
            catch
            {
                // Disposing
            }
        }
    }
}
