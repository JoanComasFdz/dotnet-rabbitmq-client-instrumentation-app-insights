using System;

namespace InstrumentedRabbitMqDotNetClient
{
    internal class RabbitMQConfiguration
    {
        public string Host { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Exchange { get; set; }

        public string QueueName { get; set; }

        public Uri Uri => new Uri($"amqp://{Host}");
    }
}