using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace InstrumentedRabbitMqDotNetClient.Connection
{
    internal class FluentConnector : IFluentConnector
    {
        private readonly ILogger<FluentConnector> _logger;
        private readonly IConnectionFactory _connectionFactory;

        public FluentConnector(
            ILogger<FluentConnector> logger,
            IConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public FluentConnectorWithDurationAndInterval TryFor(TimeSpan tryToConnectForThisPeriodOfTime)
        {
            var until = DateTime.UtcNow.Add(tryToConnectForThisPeriodOfTime);
            return new FluentConnectorWithDurationAndInterval(_logger, _connectionFactory, until);
        }
    }

    internal class FluentConnectorWithDurationAndInterval
    {
        private readonly ILogger<FluentConnector> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly DateTime _until;

        public FluentConnectorWithDurationAndInterval(
            ILogger<FluentConnector> logger,
            IConnectionFactory connectionFactory,
            in DateTime until)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _until = until;
        }

        public FluentConnectorToConnect RetryEvery(TimeSpan interval)
        {
            return new FluentConnectorToConnect(_logger, _connectionFactory, _until, interval);
        }
    }

    internal class FluentConnectorToConnect
    {
        private readonly ILogger<FluentConnector> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly DateTime _until;
        private readonly TimeSpan _interval;

        public FluentConnectorToConnect(
            ILogger<FluentConnector> logger,
            IConnectionFactory connectionFactory,
            in DateTime until,
            TimeSpan interval)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _until = until;
            _interval = interval;
        }

        public IConnection Connect()
        {
            IConnection result = null;
            Exception connectionException;
            do
            {
                connectionException = null;
                try
                {
                    result = _connectionFactory.CreateConnection();
                }
                catch (Exception e)
                {
                    _logger.LogInformation(
                        "Connection to event bus failed, trying again in {intervalInSeconds} seconds until {untilInLongTimeString}... Error is: {ConnectionError}",
                        _interval.Seconds,
                        _until.ToLongTimeString(),
                        e.Message);
                    connectionException = e;

                    Thread.Sleep(_interval);
                }

            } while (connectionException != null && _until > DateTime.UtcNow);

            if (result == null)
            {
                throw new Exception($"Could not connect to RabbitMQs after trying for '{_interval}'.");
            }

            return result;
        }
    }
}