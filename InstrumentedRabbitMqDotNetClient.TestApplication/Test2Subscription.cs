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

        public async Task HandleEventAsync(TestEvent2 theEvent, string operationId)
        {
            _logger.LogInformation("[OperationId: {operationId}] The Test2Subscription2 will handle event {event}...", operationId, theEvent.EventName);

            await Task.Delay(130);

            _logger.LogInformation("[OperationId: {operationId}] The Test2Subscription2 has finished processing event {event}.", operationId, theEvent.EventName);
        }
    }
}