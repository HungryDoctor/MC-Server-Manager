using Contracts.Lifecycle;
using Services.Lifecycle.Repository;
using Services.Lifecycle.ServerProcess;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Lifecycle.Service
{
    public class ServerService : IServerManager
    {
        private readonly IServerStateRepository m_serverStateRepository;
        private readonly IServerProcessHostFactory m_serverProcessHostFactory;


        public ServerService(IServerStateRepository serverStateRepository, IServerProcessHostFactory serverProcessHostFactory)
        {
            m_serverStateRepository = serverStateRepository;
            m_serverProcessHostFactory = serverProcessHostFactory;
        }


        public Task<ServerState> StartAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ServerState> StopAsync(ServerInstanceId serverInstanceId, bool graceful, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ServerState> GetCurrentStateAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ServerState> RestartAsync(ServerInstanceId serverInstanceId, bool graceful, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<string> AttachConsoleAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task SendCommandAsync(ServerInstanceId serverInstanceId, string command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
