namespace Services.Lifecycle
{
    public interface IServerProcessHostFactory
    {
        IServerProcessHost Create(ServerProcessStartInfo serverProcessStartInfo);
    }
}
