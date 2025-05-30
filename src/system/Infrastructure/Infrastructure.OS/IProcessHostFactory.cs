using System.IO;

namespace Infrastructure.OS
{
    public interface IProcessHostFactory
    {
        ProcessHost Create(FileInfo executable, DirectoryInfo workingDir, string? args);
    }
}
