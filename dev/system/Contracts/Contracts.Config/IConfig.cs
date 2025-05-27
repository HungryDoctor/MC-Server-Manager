namespace Contracts.Config
{
    public interface IConfig
    {
        Task<GlobalSettings> GetAsync(CancellationToken ct = default);
        Task SaveAsync(GlobalSettings settings, CancellationToken ct = default);
    }
}
