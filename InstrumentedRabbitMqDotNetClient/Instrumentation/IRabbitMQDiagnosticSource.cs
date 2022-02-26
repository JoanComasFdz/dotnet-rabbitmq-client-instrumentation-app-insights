using System.Diagnostics;
using RabbitMQ.Client;

namespace InstrumentedRabbitMqDotNetClient.Instrumentation
{
    /// <summary>
    /// <para>
    /// Adds methods to create instrumentation events so that other frameworks can act on the actions here performed.
    /// </para>
    /// </summary>
    internal interface IRabbitMQDiagnosticSource
    {
        /// <summary>
        /// Creates an Start activity and tells a diagnostic listener to start it.
        /// </summary>
        /// <param name="eventName">The name (or routingKey) of the event being published.</param>
        /// <param name="payload">The data to be sent (or event content).</param>
        /// <returns>The activity already started.</returns>
        Activity StartSend(string eventName, string payload);

        /// <summary>
        /// Creates a Process activity and tells the diagnostic listener to start it.
        /// </summary>
        /// <param name="eventName">The name (or routingKey) of the event being processed.</param>
        /// <param name="payload">The data received (or event content).</param>
        /// <param name="parentActivityId">The id of the activity of the application that published the event.</param>
        /// <returns></returns>
        Activity StartProcess(string eventName, string payload, string parentActivityId);

        /// <summary>
        /// <para>
        /// T ells the diagnostic listener to stop an activity and disposes the activity.
        /// </para>
        /// <para>
        /// Use the methods <see cref="StartSend"/> and <see cref="StartProcess"/> to create an activity. Process your actions. Then call this method
        /// with that same activity.
        /// </para>
        /// </summary>
        /// <param name="activity">The activity to be stopped.</param>
        /// <param name="failure">Indicates whether there was a failure while processing your actions.</param>
        void Stop(Activity activity, bool failure = false);
    }
}