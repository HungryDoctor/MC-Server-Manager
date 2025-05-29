namespace Contracts.Configuration
{
    public sealed record TurnOffMessage(
        TimeSpan BeforeStop,
        string Message);
}
