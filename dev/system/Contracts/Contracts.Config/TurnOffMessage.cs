namespace Contracts.Config
{
    public sealed record TurnOffMessage(
        TimeSpan BeforeStop,
        string Message);
}
