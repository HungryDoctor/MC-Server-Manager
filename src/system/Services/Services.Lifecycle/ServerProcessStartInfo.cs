namespace Services.Lifecycle
{
    public record class ServerProcessStartInfo(string JdkFullPath, string ServerJarFullPath, string ServerArgs);
}
