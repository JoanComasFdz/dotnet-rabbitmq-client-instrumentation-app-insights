using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace InstrumentedRabbitMqDotNetClient.TestApplication
{
    internal class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
    {
        private string _cloudRoleName;

        public CloudRoleNameTelemetryInitializer(string cloudRoleName)
        {
            this._cloudRoleName = cloudRoleName;
        }
        public void Initialize(ITelemetry telemetry)
        {
            // set custom role name here
            telemetry.Context.Cloud.RoleName = this._cloudRoleName;
        }
    }
}