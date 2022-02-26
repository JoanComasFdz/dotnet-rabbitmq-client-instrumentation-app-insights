using RabbitMQ.Client;

namespace InstrumentedRabbitMqDotNetClient
{
    internal interface IEventBusChannelProvider
    {
        IModel GetChannel();
    }
}