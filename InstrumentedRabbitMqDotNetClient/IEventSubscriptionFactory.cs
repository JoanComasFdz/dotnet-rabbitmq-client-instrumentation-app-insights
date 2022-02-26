using System;
using System.Collections.Generic;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient
{
    internal interface IEventSubscriptionFactory
    {
        IEnumerable<string> EventNames { get; }

        IEventSubscription CreateEventBusSubscription(IServiceProvider serviceProvider, string eventName);
    }
}