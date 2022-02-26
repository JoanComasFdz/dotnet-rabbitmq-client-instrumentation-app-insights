using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.Publishing
{
    /// <summary>
    /// <para>
    /// Allows to publish an event in the event bus.
    /// </para>
    /// <para>
    /// Get this interface injected in your class whenever an event needs to be publish.
    /// </para>
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes an event in the event bus.
        /// </summary>
        /// <param name="theEvent">The event to be published. It will be serialized as JSON before passed as payload.</param>
        void Publish(IEvent theEvent);
    }
}