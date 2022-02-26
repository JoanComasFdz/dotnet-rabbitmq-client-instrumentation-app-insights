using System.Diagnostics;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace InstrumentedRabbitMqDotNetClient.Instrumentation
{
    /// <summary>
    /// <para>
    /// Creates activities and tells a <see cref="DiagnosticListener"/> to start or stop them.
    /// </para>
    /// <para>
    /// Application Insights is automatically tracking events from "Microsoft.Azure.ServiceBus" and sending them correctly to Azure.
    /// To avoid creating all the necessary code to receive the events, we just mimic ServiceBus and let Application Insights to the job.
    /// </para>
    /// </summary>
    internal class RabbitMQDiagnosticSource : IRabbitMQDiagnosticSource
    {
        private static readonly DiagnosticListener DiagnosticListener = new DiagnosticListener(RabbitMQDiagnosticSourceName);

        private readonly RabbitMQConfiguration _config;

        public const string RabbitMQDiagnosticSourceName = "Microsoft.Azure.ServiceBus";


        public RabbitMQDiagnosticSource(RabbitMQConfiguration config)
        {
            _config = config;
        }

        public Activity StartSend(string eventName, string payload)
        {
            var sendEventName = $"{RabbitMQDiagnosticSourceName}.Send";
            var activity = new Activity(sendEventName);
            activity.AddTag("MessageId", eventName);
            activity.AddTag("MessageContent", payload);
            var activityPayload = new
            {
                Messages = new[] { activity },
                Entity = _config.Exchange,
                Endpoint = _config.Uri,
            };

            DiagnosticListener.StartActivity(activity, activityPayload);

            return activity;
        }

        public Activity StartProcess(string eventName, string payload, string parentActivityId)
        {
            var processEventName = $"{RabbitMQDiagnosticSourceName}.Process";
            var activity = new Activity(processEventName);
            activity.SetParentId(parentActivityId);
            activity.AddTag("MessageId", eventName);
            activity.AddTag("MessageContent", payload);
            var activityPayload = new
            {
                Messages = new[] { activity },
                Entity = _config.QueueName,
                Endpoint = _config.Uri,
            };
            DiagnosticListener.StartActivity(activity, activityPayload);

            return activity;
        }

        public void Stop(Activity activity, bool failure = false)
        {
            var activityPayload = new
            {
                Messages = new[] { activity },
                Entity = _config.QueueName,
                Endpoint = _config.Uri,
                Status = failure ? TaskStatus.Faulted : TaskStatus.RanToCompletion
            };

            DiagnosticListener.StopActivity(activity, activityPayload);
            activity.Dispose();
        }
    }
}