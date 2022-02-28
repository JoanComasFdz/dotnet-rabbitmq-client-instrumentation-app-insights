using System;
using System.Threading.Tasks;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.Subscription;

internal class EventSubscriptionWrapper : IEventSubscription<IEvent>
{
    private readonly object _subscriptionInstance;

    public EventSubscriptionWrapper(Type subscriptionType, object subscriptionInstance)
    {
        SubscriptionType = subscriptionType;
        _subscriptionInstance = subscriptionInstance;
    }

    public Type SubscriptionType { get; private set; }

    public Task HandleEventAsync(IEvent receivedEvent, string operationId)
    {
        return (Task) _subscriptionInstance
            .GetType()
            .GetMethod("HandleEventAsync")
            .Invoke(_subscriptionInstance, new object[]{ receivedEvent, operationId });
    }
}