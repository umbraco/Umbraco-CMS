using System;
using Umbraco.Core;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// The attribute to assign to any ActionMenuItem objects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ActionMenuItemAttribute : Attribute
    {
        /// <summary>
        /// This constructor defines both the angular service and method name to use
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        public ActionMenuItemAttribute(string serviceName, string methodName)
        {
            Mandate.ParameterNotNullOrEmpty(serviceName, "serviceName");
            Mandate.ParameterNotNullOrEmpty(methodName, "methodName");
            MethodName = methodName;
            ServiceName = serviceName;
        }

        /// <summary>
        /// This constructor will assume that the method name equals the type name of the action menu class
        /// </summary>
        /// <param name="serviceName"></param>
        public ActionMenuItemAttribute(string serviceName)
        {
            Mandate.ParameterNotNullOrEmpty(serviceName, "serviceName");
            MethodName = "";
            ServiceName = serviceName;
        }

        public string MethodName { get; private set; }
        public string ServiceName { get; private set; }
    }
}