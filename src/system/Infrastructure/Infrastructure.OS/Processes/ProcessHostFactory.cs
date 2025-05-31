using Microsoft.Extensions.Logging;
using System.IO;

namespace Infrastructure.OS.Processes
{
    public class ProcessHostFactory : IProcessHostFactory
    {
        private readonly ILoggerFactory _loggerFactory;


        public ProcessHostFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }


        public ProcessHost Create(FileInfo executable, DirectoryInfo workingDir, string? args)
        {
            ILogger<ProcessHost> logger = _loggerFactory.CreateLogger<ProcessHost>();
            return new ProcessHost(logger, executable, workingDir, args);
        }
    }
}
