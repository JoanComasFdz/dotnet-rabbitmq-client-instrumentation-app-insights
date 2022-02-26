using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    public class Test2Subscription : IEventSubscription<TestEvent2>
    {
        private readonly ILogger _logger;

        public Test2Subscription(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Test2Subscription>();
        }

        public async Task HandleEventAsync(TestEvent2 theEvent)
        {
            _logger.LogInformation("The Test2Subscription will handle event {event}...", theEvent.EventName);

            await Task.Delay(130);

            _logger.LogInformation("The Test2Subscription has finished processing event {event}.", theEvent.EventName);
        }
    }
}