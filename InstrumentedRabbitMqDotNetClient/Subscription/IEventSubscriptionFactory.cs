using System;
using System.Collections.Generic;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.Subscription
{
    internal interface IEventSubscriptionFactory
    {
        IEnumerable<string> EventNames { get; }

        IEventSubscription CreateEventBusSubscription(IServiceProvider serviceProvider, string eventName);
    }
}