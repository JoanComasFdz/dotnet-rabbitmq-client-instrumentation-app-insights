using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InstrumentedRabbitMqDotNetClient.Connection;
using InstrumentedRabbitMqDotNetClient.Contracts;
using InstrumentedRabbitMqDotNetClient.Instrumentation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace InstrumentedRabbitMqDotNetClient
{
    internal class RabbitMQSubscriberHostedService : BackgroundService
    {
        private readonly RabbitMQConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSubscriptionFactory _eventSubscriptionFactory;
        private readonly IFluentConnector _connector;
        private readonly IRabbitMQDiagnosticSource _rabbitMQDiagnosticSource;

        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;

        public RabbitMQSubscriberHostedService(
            RabbitMQConfiguration configuration,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            IEventSubscriptionFactory eventSubscriptionFactory,
            IFluentConnector connector,
            IRabbitMQDiagnosticSource rabbitMQDiagnosticSource)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<RabbitMQSubscriberHostedService>();
            _serviceProvider = serviceProvider;
            _eventSubscriptionFactory = eventSubscriptionFactory;
            _connector = connector;
            _rabbitMQDiagnosticSource = rabbitMQDiagnosticSource;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            Connect();

            ConfigureConsumer();

            AddSubscriptions();

            return Task.CompletedTask;
        }

        private void Connect()
        {
            _logger.LogDebug("Connecting to RabbitMQ...");

            _connection = _connector
                .TryFor(TimeSpan.FromMinutes(3))
                .RetryEvery(TimeSpan.FromSeconds(10))
                .Connect();

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _configuration.Exchange, type: ExchangeType.Topic, true);
            _channel.QueueDeclare(_configuration.QueueName, true, false);
            _channel.BasicQos(0, 1, false);
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            _logger.LogInformation("Connected to RabbitQM at '{Host}' exchange '{Exchange}' queue '{QueueName}'.",
                _configuration.Host,
                _configuration.Exchange,
                _configuration.QueueName);
        }

        private void ConfigureConsumer()
        {
            _logger.LogDebug("Configuring consumer...");
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += ReceivedEventHandler;
            _consumer.Shutdown += OnConsumerShutdown;
            _consumer.Registered += OnConsumerRegistered;
            _consumer.Unregistered += OnConsumerUnregistered;
            _consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(_configuration.QueueName, false, _consumer);

            _logger.LogDebug("Consumer configured.");
        }

        private void AddSubscriptions()
        {
            foreach (var eventName in _eventSubscriptionFactory.EventNames)
            {
                _logger.LogDebug("Binding queue '{QueueName}' on exchange '{Exchange}' to event '{EventName}'...",
                    _configuration.QueueName,
                    _configuration.Exchange,
                    eventName);
                _channel.QueueBind(queue: _configuration.QueueName, exchange: _configuration.Exchange, routingKey: eventName);
            }

            _logger.LogInformation("All event subscriptions bound!");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        private void ReceivedEventHandler(object model, BasicDeliverEventArgs ea)
        {
            var eventName = ea.RoutingKey;
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var operationId = GetOperationId(ea);

            _logger.LogDebug("Received {eventName} with Operation Id {RequestId}. Processing...",
                eventName,
                operationId);

            var activity = _rabbitMQDiagnosticSource.StartProcess(
                eventName,
                message,
                operationId);

            HandleMessage(operationId, eventName, message)
                .ContinueWith(a =>
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _rabbitMQDiagnosticSource.Stop(activity, a.IsFaulted);
                    _logger.LogDebug("Finished processing {eventName} on Operation Id {OperationId} and Parent Operation Id.", eventName, operationId);
                });
        }

        private static string GetOperationId(BasicDeliverEventArgs ea)
        {
            if (ea.BasicProperties.Headers == null)
            {
                throw new InvalidRabbitMQHeaderException($"Recieved event {ea.RoutingKey} but it does not have headers. At least it should have the '{RabbitMqHeaders.CorrelationId}' header, which is required to perform proper tracing.");
            }

            if (!ea.BasicProperties.Headers.ContainsKey(RabbitMqHeaders.CorrelationId))
            {
                throw new InvalidRabbitMQHeaderException($"Recieved event {ea.RoutingKey} but it does not have a '{RabbitMqHeaders.CorrelationId}' header, which is required to perform proper tracing.");
            }

            string operationId;
            try
            {
                var requestIdByteArray = (byte[])ea.BasicProperties.Headers?[RabbitMqHeaders.CorrelationId];
                operationId = Encoding.UTF8.GetString(requestIdByteArray);
            }
            catch (Exception ex)
            {
                throw new InvalidRabbitMQHeaderException($"Recieved event {ea.RoutingKey} but could not read header '{RabbitMqHeaders.CorrelationId}', make sure the publisher added the header as string.", ex);
            }

            return operationId;
        }

        private async Task HandleMessage(string requestId, string eventName, string content)
        {
            using var scope = _serviceProvider.CreateScope();
            using var loggerSCope = _logger.BeginScope("{@RequestId}", requestId);
            IEventSubscription subscription = default;
            try
            {
                subscription = _eventSubscriptionFactory.CreateEventBusSubscription(scope.ServiceProvider, eventName);
                await subscription.HandleEventAsync(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error executing event handler '{Handler}' for message '{Content}': {ExceptionType}:{ExceptionMessage}.\r\nStackTrace:\r\n{LibEventBusMessageSubscriptionStackTrace}",
                    subscription.GetType().Name,
                    content,
                    ex.GetType(),
                    ex.Message,
                    ex.StackTrace);
                throw;
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation("InstrumentedRabbitMqDotNetClient is disposing...");
            _consumer.Received -= ReceivedEventHandler;
            _consumer.Shutdown -= OnConsumerShutdown;
            _consumer.Registered -= OnConsumerRegistered;
            _consumer.Unregistered -= OnConsumerUnregistered;
            _consumer.ConsumerCancelled -= OnConsumerConsumerCancelled;

            _connection.ConnectionShutdown -= RabbitMQ_ConnectionShutdown;

            _channel.Close();
            _connection.Close();

            _logger.LogInformation("InstrumentedRabbitMqDotNetClient is disposed.");
            base.Dispose();
        }
    }
}