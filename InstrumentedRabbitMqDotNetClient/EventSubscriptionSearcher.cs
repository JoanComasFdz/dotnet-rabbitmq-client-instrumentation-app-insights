using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient;

internal class EventSubscriptionSearcher
{
    public static IReadOnlyCollection<Type> GetEventSubscriptionTypes()
    {
        var callingAssembly = Assembly
            .GetEntryAssembly();
        Console.WriteLine($"Calling assembly is '{callingAssembly.FullName}'");

        // Source: https://stackoverflow.com/questions/8645430/get-all-types-implementing-specific-open-generic-type
        var subscriptionTypes = (from x in callingAssembly.GetTypes()
                from z in x.GetInterfaces()
                let y = x.BaseType
                where
                    (y != null && y.IsGenericType &&
                     typeof(IEventSubscription<>).IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                    (z.IsGenericType &&
                     typeof(IEventSubscription<>).IsAssignableFrom(z.GetGenericTypeDefinition()))
                select x)
            .ToArray();

        return subscriptionTypes;
    }
}