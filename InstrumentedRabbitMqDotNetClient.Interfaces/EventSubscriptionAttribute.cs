using System;

namespace InstrumentedRabbitMqDotNetClient.Contracts
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EventSubscriptionAttribute : Attribute
    {
        public EventSubscriptionAttribute(string eventName)
        {
            EventName = eventName;
        }
        public string EventName { get; }
    }
}