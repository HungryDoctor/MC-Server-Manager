using Contracts.Lifecycle;
using Services.Lifecycle.Repository;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Lifecycle.Service
{
    public class ServerService : IServerManager
    {
        private readonly IServerStateRepository m_serverStateRepository;


        public ServerService(IServerStateRepository serverStateRepository)
        {
            m_serverStateRepository = serverStateRepository;
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
