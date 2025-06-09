namespace Services.Lifecycle
{
    public interface IServerProcessHostFactory
    {
        ServerProcessHost Create(ServerProcessStartInfo serverProcessStartInfo);
    }
}
