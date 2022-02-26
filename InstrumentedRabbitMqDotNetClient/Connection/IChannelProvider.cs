using RabbitMQ.Client;

namespace InstrumentedRabbitMqDotNetClient.Connection
{
    internal interface IChannelProvider
    {
        IModel GetChannel();
    }
}