using RabbitMQ.Client;

namespace InstrumentedRabbitMqDotNetClient
{
    internal class EventBusChannelProvider : IEventBusChannelProvider
    {
        private readonly IModel _channel;

        public EventBusChannelProvider(RabbitMQConfiguration configuration, IConnectionFactory connectionFactory)
        {
            var connection = connectionFactory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.ExchangeDeclare(exchange: configuration.Exchange, type: ExchangeType.Topic, true);
        }

        public IModel GetChannel() => _channel;
    }
}