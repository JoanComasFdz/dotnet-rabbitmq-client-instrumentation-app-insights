using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    [EventSubscription("test.event.another")]
    public class Test2Subscription : IEventSubscription
    {
        private readonly ILogger _logger;

        public Test2Subscription(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Test2Subscription>();
        }

        public async Task HandleEventAsync(string message)
        {
            _logger.LogInformation("The Test2Subscription will handle event {event}...", message);

            await Task.Delay(130);

            _logger.LogInformation("The Test2Subscription has finished processing event {event}.", message);
        }
    }
}