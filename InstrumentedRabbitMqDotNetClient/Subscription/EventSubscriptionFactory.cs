using System;
using System.Collections.Generic;
using System.Linq;
using InstrumentedRabbitMqDotNetClient.Contracts;
using Microsoft.Extensions.Logging;

namespace InstrumentedRabbitMqDotNetClient.Subscription
{
    internal class EventSubscriptionFactory : IEventSubscriptionFactory
    {
        private readonly Dictionary<string, EventSubscriptionInfo> _subscriptionTypesByEventName;
        private readonly ILogger<EventSubscriptionFactory> _logger;

        public IEnumerable<string> EventNames => _subscriptionTypesByEventName.Keys;

        public EventSubscriptionFactory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EventSubscriptionFactory>();
            _subscriptionTypesByEventName = new Dictionary<string, EventSubscriptionInfo>();

            var typesToRegister = EventSubscriptionSearcher.GetEventSubscriptionTypes();
            SaveTypesAsSubscriptions(typesToRegister);
        }
        
        private void SaveTypesAsSubscriptions(IEnumerable<Type> typesToRegister)
        {
            foreach (var subscriptionType in typesToRegister)
            {
                var eventSubscriptionInterfaceType = subscriptionType.GetInterface(typeof(IEventSubscription<>).Name);
                var eventType = eventSubscriptionInterfaceType.GetGenericArguments().First();
                var instance = Activator.CreateInstance(eventType);
                var eventName = (string)eventType.GetProperty("EventName").GetValue(instance);

                _subscriptionTypesByEventName.Add(eventName, new EventSubscriptionInfo {EventSubscriptionType = subscriptionType, EventType = eventType});
            }
        }

        public EventSubscriptionWrapper CreateEventBusSubscription(IServiceProvider serviceProvider, string eventName)
        {
            try
            {
                var subscriptionInfo = _subscriptionTypesByEventName[eventName];
                var subscriptionInstance = serviceProvider.GetService(subscriptionInfo.EventSubscriptionType);

                return new EventSubscriptionWrapper(subscriptionInfo.EventSubscriptionType, subscriptionInstance);
            }
            catch (Exception)
            {
                _logger.LogCritical("Unable to instantiate an event subscription for event {eventName}. Check if there is a type being injected in the constructor which is not properly registered in the Startup.ConfigureServices() method.", eventName);
                throw;
            }
        }

        public Type GetEventType(string eventName)
        {
            return _subscriptionTypesByEventName[eventName].EventType;
        }
    }
}