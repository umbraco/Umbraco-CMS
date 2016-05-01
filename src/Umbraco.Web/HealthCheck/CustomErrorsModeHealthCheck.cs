using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// Make sure the Custom Errors Mode is On or Remote Only
    /// </summary>
    public class CustomErrorsModeHealthCheck : HealthCheck
    {
        private const string CheckCustomErrors = "checkCustomErrors";

        public CustomErrorsModeHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            var currentState = GetCurrentMode();

            return new[] {
                new HealthCheckStatus(string.Format("Custom errors mode is {0}", currentState))
                {
                    ResultType = currentState == CustomErrorsMode.Off
                                    ? StatusResultType.Error
                                    : StatusResultType.Success,
                    Actions = new[]
                    {
                        new HealthCheckAction(CheckCustomErrors, Id)
                    }
                }
            };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            var customErrorsMode = GetCurrentMode();
            if (customErrorsMode != CustomErrorsMode.Off)
            {
                // it's already ok
                return new HealthCheckStatus(string.Format("Custom errors was already {0}", customErrorsMode))
                {
                    ResultType = StatusResultType.Success,
                    Actions = new[]
                    {
                        new HealthCheckAction(CheckCustomErrors, Id)
                    }
                };
            }

            SetMode(CustomErrorsMode.RemoteOnly);

            return new HealthCheckStatus("Custom errors is updated")
            {
                ResultType = StatusResultType.Success,
                Actions = new[]
               {
                    new HealthCheckAction(CheckCustomErrors, Id)
                }
            };
        }

        private static void SetMode(CustomErrorsMode customErrorsMode)
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            var customErrorsection = (CustomErrorsSection)config.GetSection("system.web/customErrors");
            customErrorsection.Mode = customErrorsMode;
            config.Save();
        }

        private static CustomErrorsMode GetCurrentMode()
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            var customErrorsection = (CustomErrorsSection)config.GetSection("system.web/customErrors");
            return customErrorsection.Mode;
        }
    }
}
