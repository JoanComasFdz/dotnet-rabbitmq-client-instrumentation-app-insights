using System.Threading.Tasks;

namespace InstrumentedRabbitMqDotNetClient.Contracts
{
    public interface IEventSubscription
    {
        Task HandleEventAsync(string message);
    }
}