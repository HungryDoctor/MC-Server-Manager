namespace Contracts.Jdk
{
    public sealed record JdkUpdateStatus(
        JdkInstanceId InstanceId,
        bool HasMinorUpdate,
        bool HasMajorUpdate,
        string? LatestMinor,
        string? LatestMajor);
}
