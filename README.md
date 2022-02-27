# dotnet-rabbitmq-client-instrumentation-app-insights
A RabbitMQ client for ASP .NET Core 6 with instrumentation so that events are properly correlated in Application Insights.

![Instrumented RabbitMQ in Application Insights](https://i.imgur.com/yhpa2DR.png)

## How to use
Explore the `InstrumentedRabbitMqDotNetClient.TestApplication` to understand how to use it. It showcases this sequence:

`PublishEventController` => Publishes `TestEvent` => `TestSubscription` receives it and publishes `TestEvent2` => `Test2Subscription` receives it.

### Register it in Startup
1. In the `Startup` class, add:
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other initialization logc
        services.AddRabbitMQSubscriberHostedService("name-of-the-queue");
    }
}
```

### Create an event
1. Declare a `record` that inherits from `IEvent`.
```csharp
public record MyEvent : IEvent
{
    public string EventName => "my.event";
}
```
### Publish an event
1. Inject `IEventPublisher` in a class:
```csharp
public class MyClass
{
    private readonly IEventPublisher _eventPublisher;

    public MyClass(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public void DoSomething()
    {
        this._eventPublisher.Publish(new MyEvent())
    }
}
```

### Subscribe to an event
1. Create a class to inherit from `IEventSubscription<MyEvent>`:
```csharp
public class MyEventSubscription : IEventSubscription<MyEvent>
{
    public Task HandleEventAsync(MyEvent receivedEvent)
    {
        // Your logic here.
    }
}
```

## Instrumentation
The aporoach is to emualte [Azure Service Bus](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/servicebus/Microsoft.Azure.ServiceBus/src), so that Application Insights shows the nested messages and adds some information about the queue as shown in the image above.

But you do not have to do anything besides configure the Application Insights connection string.

[More info](https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-operations-tracking)

### How it works
A [DiagnosticSource](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md) is available in the `Instrumentation` folder (It is a simplification of the [ServiceBusDiagnosticSource](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Microsoft.Azure.ServiceBus/src/ServiceBusDiagnosticsSource.cs)).

The `EventPublisher` uses it to start the activity.

The `RabbitMQSubscriberHostedService` uses it to start processing the event and to to signal that the event processing has finished.

### Set connection string
1. [Create an Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource).
2. Copy the connection string.
3. Set it in the env var `APPLICATIONINSIGHTS_CONNECTION_STRING`. When debugging, it is in the `launchSettings.json` file in th Properties folder of the TestApplication.