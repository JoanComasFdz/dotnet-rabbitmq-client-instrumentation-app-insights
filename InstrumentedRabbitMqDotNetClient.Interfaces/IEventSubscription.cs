using System.Threading.Tasks;

namespace InstrumentedRabbitMqDotNetClient.Contracts
{
    /// <summary>
    /// Implement this interface in order to subscribe to an event.
    /// <para>
    /// No need to register it anywhere.
    /// </para>
    /// <para>
    /// A new instance of the implementing class will be created for every event.
    /// </para>
    /// <para>
    /// Any type registered in the IServiceProvider can be injected in the constructor.
    /// </para>
    /// </summary>
    /// <typeparam name="TEvent">The event to subscribe to.</typeparam>
    public interface IEventSubscription<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Implement the logic needed to handle the specified event.
        /// </summary>
        /// <param name="receivedEvent">An instance of the event already parsed.</param>
        /// <param name="operationId">The id that uniquely identifies the whole trace.</param>
        /// <returns>A Task, so that the call can be awaited.</returns>
        Task HandleEventAsync(TEvent receivedEvent, string operationId);
    }
}