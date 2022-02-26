using System;
using Microsoft.AspNetCore.Mvc;

namespace InstrumentedRabbitMqDotNetClient.TestApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PublishEventController : ControllerBase
    {
        private readonly IEventPublisher _eventPublisher;

        public PublishEventController(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        [HttpGet]
        public string Get()
        {
            _eventPublisher.Publish(new TestEvent());
            return $"Event Published at {DateTime.Now.ToLongDateString()} - {DateTime.Now.ToLongTimeString()}";
        }
    }
}
