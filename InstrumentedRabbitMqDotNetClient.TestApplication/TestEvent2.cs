﻿using System;
using InstrumentedRabbitMqDotNetClient.Contracts;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    public class TestEvent2 : IEvent
    {
        public string EventName => "test.event.another";

        public int HitCount => 954;

        public bool HasBooked => false;

        public DateTime DueDate => DateTime.Parse("01.02.2100");
    }
}