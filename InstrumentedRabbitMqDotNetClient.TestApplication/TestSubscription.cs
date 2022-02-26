using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    [EventSubscription("test.event")]
    public class TestSubscription : IEventSubscription
    {
        private readonly IEventPublisher _publisher;
        private readonly ILogger _logger;

        public TestSubscription(ILoggerFactory loggerFactory, IEventPublisher publisher)
        {
            _publisher = publisher;
            _logger = loggerFactory.CreateLogger<TestSubscription>();
        }

        public Task HandleEventAsync(string message)
        {
            _logger.LogInformation("The TestSubscription will handle event {event}...", message);

            _publisher.Publish(new TestEvent2());

            _logger.LogInformation("The TestSubscription has finished processing event {event}.", message);

            return Task.CompletedTask;
        }
    }
}