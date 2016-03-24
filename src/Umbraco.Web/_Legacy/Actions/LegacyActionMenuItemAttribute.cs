using System;
using Umbraco.Core;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// The attribute to assign to any IAction objects.
    /// </summary>
    /// <remarks>
    /// This is purely used for compatibility reasons for old IActions used in v7 that haven't been upgraded to 
    /// the new format.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class LegacyActionMenuItemAttribute : Attribute
    {
        /// <summary>
        /// This constructor defines both the angular service and method name to use
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        public LegacyActionMenuItemAttribute(string serviceName, string methodName)
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
        public LegacyActionMenuItemAttribute(string serviceName)
        {
            Mandate.ParameterNotNullOrEmpty(serviceName, "serviceName");
            MethodName = "";
            ServiceName = serviceName;
        }

        public string MethodName { get; private set; }
        public string ServiceName { get; private set; }
    }
}