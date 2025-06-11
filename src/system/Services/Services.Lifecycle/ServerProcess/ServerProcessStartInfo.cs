namespace Services.Lifecycle.ServerProcess
{
    public record class ServerProcessStartInfo(string JdkFullPath, string ServerJarFullPath, string ServerArgs);
}
