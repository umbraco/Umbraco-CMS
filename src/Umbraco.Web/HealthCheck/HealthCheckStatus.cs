using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// The status returned for a health check when it performs it check
    /// TODO: This model will be used in the WebApi result so needs attributes for JSON usage
    /// </summary>
    public class HealthCheckStatus
    {
        public HealthCheckStatus(string message)
        {
            Message = message;
            Actions = Enumerable.Empty<HealthCheckAction>();
        }

        /// <summary>
        /// The status message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The status description if one is necessary
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is optional but would allow a developer to specify a path to an angular html view
        /// in order to either show more advanced information and/or to provide input for the admin
        /// to configure how an action is executed
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// The status type
        /// </summary>
        public StatusResultType ResultType { get; set; }

        /// <summary>
        /// The potential actions to take (in any)
        /// </summary>
        public IEnumerable<HealthCheckAction> Actions { get; set; }

        //TODO: What else?
    }
}