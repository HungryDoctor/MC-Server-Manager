using Contracts.Lifecycle;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Lifecycle.Repository
{
    public class LiteDbServerProcessRepository : IServerProcessRepository
    {
        private const string c_serverProcess = "server_process";
        private readonly ILiteDatabase m_db;


        public LiteDbServerProcessRepository(ILiteDatabase db)
        {
            m_db = db;
        }


        public Task<ServerInstanceId> CreateAsync(ServerState serverState, CancellationToken ct = default)
        {
            m_db.GetCollection<ServerState>(c_serverProcess).Insert(serverState.GetBsonKey(), serverState);
            return Task.FromResult(serverState.ServerInstanceId);
        }

        public Task<IReadOnlySet<ServerState>> GetAsync(CancellationToken ct = default)
        {
            IReadOnlySet<ServerState> result = m_db.GetCollection<ServerState>(c_serverProcess).FindAll().ToHashSet();
            return Task.FromResult(result);
        }

        public Task<ServerState?> GetAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default)
        {
            ServerState? result = m_db.GetCollection<ServerState>(c_serverProcess).FindById(serverInstanceId.GetBsonKey());
            return Task.FromResult(result);
        }

        public Task UpdateAsync(ServerState serverState, CancellationToken ct = default)
        {
            m_db.GetCollection<ServerState>(c_serverProcess).Upsert(serverState.GetBsonKey(), serverState);
            return Task.CompletedTask;
        }
    }

    file static class ServerInstanceIdExtensions
    {
        public static BsonValue GetBsonKey(this ServerInstanceId serverInstanceId) => serverInstanceId.Value;
        public static BsonValue GetBsonKey(this ServerState serverState) => serverState.ServerInstanceId.Value;
    }
}
