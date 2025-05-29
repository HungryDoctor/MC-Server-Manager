namespace Contracts.Jdk
{
    public sealed record JdkInfo(
        JdkInstanceId InstanceId,
        JdkKind Kind,
        string Vendor,
        string Version,
        string Path);
}
