using Infrastructure.OS.Processes;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Services.Lifecycle
{
    public class ServerProcessHostFactory : IServerProcessHostFactory
    {
        private readonly ILoggerFactory m_loggerFactory;


        public ServerProcessHostFactory(ILoggerFactory loggerFactory)
        {
            m_loggerFactory = loggerFactory;
        }


        public IServerProcessHost Create(ServerProcessStartInfo serverProcessStartInfo)
        {
            ProcessHost processHost = CreateProcessHost(serverProcessStartInfo);
            ServerProcessHost serverProcessHost = CreateServerProcessHost(processHost);
            return serverProcessHost;
        }

        private ProcessHost CreateProcessHost(ServerProcessStartInfo serverProcessStartInfo)
        {
            ILogger<ProcessHost> logger = m_loggerFactory.CreateLogger<ProcessHost>();
            FileInfo executable = new FileInfo(serverProcessStartInfo.JdkFullPath);
            string serverDirName = Path.GetDirectoryName(serverProcessStartInfo.ServerJarFullPath)!;
            DirectoryInfo directoryInfo = new DirectoryInfo(serverDirName);
            string args = $"-jar \"{serverProcessStartInfo.ServerJarFullPath}\" {serverProcessStartInfo.ServerArgs}".Trim();

            ProcessHost processHost = new ProcessHost(logger, executable, directoryInfo, args);
            return processHost;
        }

        private ServerProcessHost CreateServerProcessHost(ProcessHost processHost)
        {
            ILogger<ServerProcessHost> logger = m_loggerFactory.CreateLogger<ServerProcessHost>();
            return new ServerProcessHost(logger, processHost);
        }
    }
}
