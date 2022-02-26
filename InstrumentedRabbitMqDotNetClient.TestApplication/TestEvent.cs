using System;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    public record TestEvent : IEvent
    {
        public string EventName => "test.event";

        public int Age => 19;

        public bool IsLol => true;

        public DateTime MyBirthday => DateTime.Parse("01.02.1989");
    }
}