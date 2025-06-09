namespace Services.Lifecycle.ServerProcess
{
    public interface IServerProcessHostFactory
    {
        IServerProcessHost Create(ServerProcessStartInfo serverProcessStartInfo);
    }
}
