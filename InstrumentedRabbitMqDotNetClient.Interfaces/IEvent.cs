namespace InstrumentedRabbitMqDotNetClient.Contracts
{
    public interface IEvent
    {
        string EventName { get; }
    }
}