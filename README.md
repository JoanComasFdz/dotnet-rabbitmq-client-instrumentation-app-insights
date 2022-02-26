# dotnet-rabbitmq-client-instrumentation-app-insights
A RabbitMQ client for ASP .NET Core 6 with instrumentation so that events are properly correlated in Application Insights.

![Instrumented RabbitMQ in Application Insights](https://i.imgur.com/yhpa2DR.png)

## How to use
Explore the `InstrumentedRabbitMqDotNetClient.TestApplication` to understand how to use it. It showcases this sequence:

`PublishEventController` => Publishes `TestEvent` => `TestSubscription` receives it and publishes `TestEvent2` => `Test2Subscription` receives it.

### Application Insights Connection String
1. Create an Azure Application Insights.
2. Copy Connection string.
3. Set it in the env var `APPLICATIONINSIGHTS_CONNECTION_STRING`. When debugging, make sure to have it in the launchSettings.json

### Register it in Startup
1. In `Startup.ConfigureServices()` add:
```
services.AddRabbitMQSubscriberHostedService("name-of-the-queue");
```

### Create an event
1. Declare a `record` that inherits from `IEvent`.
```
public record MyEvent : IEvent
{
    public string EventName => "my.event";
}
```
### Publish an event
1. Inject `IEventPublisher` in a class:
```
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
```
public class MyEventSubscription : IEventSubscription<MyEvent>
{
    public Task HandleEventAsync(MyEvent receivedEvent)
    {
        // Your logic here.
    }
}
```

