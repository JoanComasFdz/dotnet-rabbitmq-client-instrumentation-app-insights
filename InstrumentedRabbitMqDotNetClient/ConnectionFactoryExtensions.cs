using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace InstrumentedRabbitMqDotNetClient
{
    internal class ConnectionFactoryConnector : IConnectionFactoryConnector
    {
        private readonly ILogger<ConnectionFactoryConnector> _logger;
        private readonly IConnectionFactory _connectionFactory;

        public ConnectionFactoryConnector(ILogger<ConnectionFactoryConnector> logger, IConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public ConnectionFactoryConnectorWithDurationAndInterval TryFor(TimeSpan tryToConnectForThisPeriodOfTime)
        {
            var until = DateTime.UtcNow.Add(tryToConnectForThisPeriodOfTime);
            return new ConnectionFactoryConnectorWithDurationAndInterval(_logger, _connectionFactory, until);
        }
    }

    internal class ConnectionFactoryConnectorWithDurationAndInterval
    {
        private readonly ILogger<ConnectionFactoryConnector> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly DateTime _until;

        public ConnectionFactoryConnectorWithDurationAndInterval(
            ILogger<ConnectionFactoryConnector> logger,
            IConnectionFactory connectionFactory,
            in DateTime until)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _until = until;
        }

        public ConnectionFactoryConnectorExecuter RetryEvery(TimeSpan interval)
        {
            return new ConnectionFactoryConnectorExecuter(_logger, _connectionFactory, _until, interval);
        }
    }

    internal class ConnectionFactoryConnectorExecuter
    {
        private readonly ILogger<ConnectionFactoryConnector> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly DateTime _until;
        private readonly TimeSpan _interval;

        public ConnectionFactoryConnectorExecuter(ILogger<ConnectionFactoryConnector> logger,
            IConnectionFactory connectionFactory, in DateTime until, TimeSpan interval)
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
                    _logger.LogInformation("Connection to event bus failed, trying again in {intervalInSeconds} seconds until {untilInLongTimeString}... Error is: {ConnectionError}",
                        _interval.Seconds, _until.ToLongTimeString(), e.Message);
                    connectionException = e;

                    Thread.Sleep(_interval);
                }

            } while (connectionException != null && _until > DateTime.UtcNow);

            if (result == null)
            {
                throw new Exception($"Could not connect to event bus after trying for '{_interval}'.");
            }

            return result;
        }
    }
}