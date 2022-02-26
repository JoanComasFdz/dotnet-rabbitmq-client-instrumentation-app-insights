using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    /// <summary>
    /// This class adds the Cloud Role Name to all telemetry sent to Application Insights.
    /// <para>
    /// Basically, the microservice name.
    /// </para>
    /// </summary>
    internal class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _cloudRoleName;

        public CloudRoleNameTelemetryInitializer(string cloudRoleName)
        {
            this._cloudRoleName = cloudRoleName;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = this._cloudRoleName;
        }
    }
}