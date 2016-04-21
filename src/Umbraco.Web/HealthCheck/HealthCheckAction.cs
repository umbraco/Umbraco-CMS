using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.HealthCheck
{
    [DataContract(Name = "healthCheckAction", Namespace = "")]
    public class HealthCheckAction
    {

        /// <summary>
        /// Empty ctor used for serialization
        /// </summary>
        public HealthCheckAction()
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="healthCheckId"></param>
        public HealthCheckAction(string alias, Guid healthCheckId)
        {
            Alias = alias;
            HealthCheckId = healthCheckId;
        }

        /// <summary>
        /// The alias of the action - this is used by the Health Check instance to execute the action
        /// </summary>
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// The name of the action - this is shown as the label of the button used for triggering the action.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the action - this is shown as the tooltip of the button used for triggering the action.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// The Id of the Health Check instance
        /// </summary>
        /// <remarks>
        /// This is used to find the Health Check instance to execute this action
        /// </remarks>
        [DataMember(Name = "healthCheckId")]
        public Guid HealthCheckId { get; set; }

        /// <summary>
        /// This could be used if the status has a custom view that specifies some parameters to be sent to the server
        /// when an action needs to be executed
        /// </summary>
        [DataMember(Name = "actionParameters")]
        public Dictionary<string, object> ActionParameters { get; set; }
    }
}