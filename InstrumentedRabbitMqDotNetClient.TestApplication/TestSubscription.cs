using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InstrumentedRabbitMqDotNetClient.Contracts;
using InstrumentedRabbitMqDotNetClient.Publishing;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    public class TestSubscription : IEventSubscription<TestEvent>
    {
        private readonly IEventPublisher _publisher;
        private readonly ILogger _logger;

        public TestSubscription(ILoggerFactory loggerFactory, IEventPublisher publisher)
        {
            _publisher = publisher;
            _logger = loggerFactory.CreateLogger<TestSubscription>();
        }

        public Task HandleEventAsync(TestEvent theEvent, string operationId)
        {
            _logger.LogInformation("[OperationId: {operationId}] The TestSubscription will handle event {event}...", operationId, theEvent.EventName);

            _publisher.Publish(new TestEvent2());

            _logger.LogInformation("[OperationId: {operationId}] The TestSubscription has finished processing event {event}.", operationId, theEvent.EventName);

            return Task.CompletedTask;
        }
    }
}