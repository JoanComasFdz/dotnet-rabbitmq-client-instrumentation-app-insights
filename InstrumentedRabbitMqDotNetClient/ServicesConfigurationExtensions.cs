using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using InstrumentedRabbitMqDotNetClient.Contracts;
using InstrumentedRabbitMqDotNetClient.Instrumentation;

namespace InstrumentedRabbitMqDotNetClient
{
    /// <summary>
    /// <para>
    /// Provides extension methods to configure RabbitMQ  in an instance of the <see cref="IServiceCollection"/> class.
    /// </para>
    /// </summary>
    public static class ServicesConfigurationExtensions
    {
        /// <summary>
        /// <para>
        /// Configures RabbitMQ so it automatically binds any class having the <see cref="EventSubscriptionAttribute"/> declared.
        /// </para>
        /// <para>
        /// <remarks>
        /// This method must be call in the <c>Startup.ConfigureServices()</c> method of your micro service.
        /// </remarks>
        /// </para>
        /// <para>
        /// <remarks>
        /// Requires the following environment variables:
        /// <list type="bullet">
        /// <item><term>RABBITMQ_HOST</term><description>The URL of the RabbitMQ server.</description></item>
        /// <item><term>RABBITMQ_EXCHANGE</term><description>The Exchange name to which events will be publish.</description></item>
        /// <item><term>RABBITMQ_USER</term><description></description>Username to authenticate in the RabbitMQ server when establishing a connection.</item>
        /// <item><term>RABBITMQ_PASSWORD</term><description></description>Password to authenticate in the RabbitMQ server when establishing a connection.</item>
        /// </list>
        /// </remarks>
        /// </para>
        /// </summary>
        /// <param name="services">The service collection where to register RabbitMQ.</param>
        /// <param name="queueName">The name of the queue where to read events from.</param>
        public static void AddRabbitMQSubscriberHostedService(this IServiceCollection services, string queueName)
        {
            var rabbitMQConfiguration = GetRabbitMQConfiguration(queueName);

            RegisterEventSubscriptions(services);

            services.AddSingleton(rabbitMQConfiguration);
            services.AddSingleton<IEventBusChannelProvider, EventBusChannelProvider>();
            services.AddSingleton<IEventSubscriptionFactory, EventSubscriptionFactory>();
            services.AddSingleton<IConnectionFactoryConnector, ConnectionFactoryConnector>();
            services.AddSingleton<IConnectionFactory>(new ConnectionFactory
            {
                HostName = rabbitMQConfiguration.Host,
                UserName = rabbitMQConfiguration.User,
                Password = rabbitMQConfiguration.Password
            });
            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddTransient<IRabbitMQDiagnosticSource, RabbitMQDiagnosticSource>();
            services.AddHostedService<RabbitMQSubscriberHostedService>();
        }

        private static RabbitMQConfiguration GetRabbitMQConfiguration(string queueName)
        {
            var envVars = EnvironmentVariableGetter.GetValues("RABBITMQ_HOST", "RABBITMQ_EXCHANGE", "RABBITMQ_USER", "RABBITMQ_PASSWORD");
            var rabbitMQConfiguration = new RabbitMQConfiguration
            {
                Host = envVars["RABBITMQ_HOST"],
                Exchange = envVars["RABBITMQ_EXCHANGE"],
                User = envVars["RABBITMQ_USER"],
                Password = envVars["RABBITMQ_PASSWORD"],
                QueueName = queueName
            };
            return rabbitMQConfiguration;
        }

        private static void RegisterEventSubscriptions(IServiceCollection services)
        {
            var typesToRegister = GetEventSubscriptionTypes();

            foreach (var type in typesToRegister)
            {
                services.AddTransient(type);
            }

            Console.WriteLine($"Registered '{typesToRegister.Count}' subscriptions: '{string.Join($"{Environment.NewLine} - ", typesToRegister)}'.");
        }

        private static List<Type> GetEventSubscriptionTypes()
        {
            var callingAssembly = Assembly
                .GetEntryAssembly();
            Console.WriteLine($"Calling assembly is '{callingAssembly.FullName}'");

            var typesToRegister = callingAssembly
                .GetExportedTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IEventSubscription)))
                .Select(t => t)
                .ToList();
            return typesToRegister;
        }
    }
}