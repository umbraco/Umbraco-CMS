using System;
using Umbraco.Core;
using Umbraco.Core.Exceptions;

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
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullOrEmptyException(nameof(serviceName));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullOrEmptyException(nameof(methodName));

            MethodName = methodName;
            ServiceName = serviceName;
        }

        /// <summary>
        /// This constructor will assume that the method name equals the type name of the action menu class
        /// </summary>
        /// <param name="serviceName"></param>
        public LegacyActionMenuItemAttribute(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullOrEmptyException(nameof(serviceName));

            MethodName = "";
            ServiceName = serviceName;
        }

        public string MethodName { get; }
        public string ServiceName { get; }
    }
}
