using Contracts.Lifecycle;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Lifecycle.Repository
{
    public class RavenDbServerStateRepository : IServerStateRepository
    {
        private readonly IDocumentStore m_documentStore;


        public RavenDbServerStateRepository(IDocumentStore documentStore)
        {
            m_documentStore = documentStore;
        }


        public async Task<ServerInstanceId> CreateAsync(ServerState serverState, CancellationToken ct = default)
        {
            using (IAsyncDocumentSession session = m_documentStore.OpenAsyncSession())
            {
                await session.StoreAsync(serverState, serverState.GetDbKey(), ct).ConfigureAwait(false);
                await session.SaveChangesAsync(ct).ConfigureAwait(false);

                return serverState.ServerInstanceId;
            }
        }

        public async Task<ServerState?> GetAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default)
        {
            using (IAsyncDocumentSession session = m_documentStore.OpenAsyncSession())
            {
                ServerState? result = await session.LoadAsync<ServerState>(serverInstanceId.GetDbKey(), ct).ConfigureAwait(false);

                return result;
            }
        }

        public async Task<IReadOnlySet<ServerState>> GetAsync(CancellationToken ct = default)
        {
            using (IAsyncDocumentSession session = m_documentStore.OpenAsyncSession())
            {
                HashSet<ServerState> results = new HashSet<ServerState>();
                await using (var resultEnumerable = await session.Advanced.StreamAsync<ServerState>(ServerStateDbHelper.GetCollectionKey(), token: ct))
                {
                    while (await resultEnumerable.MoveNextAsync())
                    {
                        if (ct.IsCancellationRequested)
                        {
                            ct.ThrowIfCancellationRequested();
                        }

                        var state = resultEnumerable.Current;
                        results.Add(state.Document);
                    }
                }

                return results;
            }
        }

        public async Task UpdateAsync(ServerState serverState, CancellationToken ct = default)
        {
            using (IAsyncDocumentSession session = m_documentStore.OpenAsyncSession())
            {
                await session.StoreAsync(serverState, serverState.GetDbKey(), ct).ConfigureAwait(false);
                await session.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default)
        {
            using (IAsyncDocumentSession session = m_documentStore.OpenAsyncSession())
            {
                session.Delete(serverInstanceId.GetDbKey());
                await session.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }
    }

    file static class ServerStateDbHelper
    {
        private const string c_serverProcess = "server_process";

        public static string GetCollectionKey() => c_serverProcess;
        public static string GetDbKey(this ServerState serverState) => $"{c_serverProcess}/{serverState.ServerInstanceId.Value.ToString()}";
        public static string GetDbKey(this ServerInstanceId serverInstanceId) => $"{c_serverProcess}/{serverInstanceId.Value.ToString()}";
    }
}
