using System;

namespace InstrumentedRabbitMqDotNetClient
{
    internal interface IConnectionFactoryConnector
    {
        ConnectionFactoryConnectorWithDurationAndInterval TryFor(TimeSpan tryToConnectForThisPeriodOfTime);
    }
}