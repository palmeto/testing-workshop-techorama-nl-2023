namespace EdgeCases;

internal interface IClock
{
    DateTime Now { get; }
}

internal class SystemClock : IClock
{
    public DateTime Now => DateTime.UtcNow;
}
