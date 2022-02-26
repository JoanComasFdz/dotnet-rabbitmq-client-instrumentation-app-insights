using System;

namespace InstrumentedRabbitMqDotNetClient.Subscription;

internal record EventSubscriptionInfo
{
    public Type EventSubscriptionType { get; init; }

    public Type EventType { get; init; }
}