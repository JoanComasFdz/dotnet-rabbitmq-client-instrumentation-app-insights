using System;
using System.Collections.Generic;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.Subscription
{
    internal interface IEventSubscriptionFactory
    {
        IEnumerable<string> EventNames { get; }

        EventSubscriptionWrapper CreateEventBusSubscription(IServiceProvider serviceProvider, string eventName);

        Type GetEventType(string eventName);
    }
}