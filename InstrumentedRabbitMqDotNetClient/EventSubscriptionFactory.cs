using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient
{
    internal class EventSubscriptionFactory : IEventSubscriptionFactory
    {
        private readonly Dictionary<string, Type> _subscriptionTypesByEventName;
        private readonly ILogger<EventSubscriptionFactory> _logger;

        public IEnumerable<string> EventNames => _subscriptionTypesByEventName.Keys;

        public EventSubscriptionFactory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EventSubscriptionFactory>();
            _subscriptionTypesByEventName = new Dictionary<string, Type>();

            var typesToRegister = GetTypesToRegisterAsSubscriptions();
            RegisterTypesAsSubscriptions(typesToRegister);
        }

        private static List<Type> GetTypesToRegisterAsSubscriptions()
        {
            var assembly = Assembly
                .GetEntryAssembly();

            var typesToRegister = assembly
                .GetExportedTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IEventSubscription)))
                .Select(t => t)
                .ToList();
            return typesToRegister;
        }

        private void RegisterTypesAsSubscriptions(IEnumerable<Type> typesToRegister)
        {
            foreach (var type in typesToRegister)
            {
                var eventSubscriptionAttribute = type.GetCustomAttribute<EventSubscriptionAttribute>();
                if (eventSubscriptionAttribute == null)
                {
                    throw new InvalidOperationException(
                        $"Event subscription in type {type.Name} is missing EventSubscription attribute");
                }

                _subscriptionTypesByEventName.Add(eventSubscriptionAttribute.EventName, type);
            }
        }

        public IEventSubscription CreateEventBusSubscription(IServiceProvider serviceProvider, string eventName)
        {
            try
            {
                var subscriptionType = _subscriptionTypesByEventName[eventName];
                var service = serviceProvider.GetService(subscriptionType);
                var eventBusSubscription = (IEventSubscription)service;
                return eventBusSubscription;
            }
            catch (Exception)
            {
                _logger.LogCritical("Unable to instantiate an event subscription for event {eventName}. Check if there is a type being injected in the constructor which is not properly registered in the Startup.ConfigureServices() method.", eventName);
                throw;
            }

        }
    }
}