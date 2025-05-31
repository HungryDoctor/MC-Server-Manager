using System.IO;

namespace Infrastructure.OS.Processes
{
    public interface IProcessHostFactory
    {
        ProcessHost Create(FileInfo executable, DirectoryInfo workingDir, string? args);
    }
}
