using System;

namespace InstrumentedRabbitMqDotNetClient.Connection
{
    internal interface IFluentConnector
    {
        FluentConnectorWithDurationAndInterval TryFor(TimeSpan tryToConnectForThisPeriodOfTime);
    }
}