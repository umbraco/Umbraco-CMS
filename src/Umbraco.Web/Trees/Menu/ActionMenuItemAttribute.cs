using System;

namespace Umbraco.Web.Trees.Menu
{
    /// <summary>
    /// The attribute to assign to any ActionMenuItem objects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ActionMenuItemAttribute : Attribute
    {
        public ActionMenuItemAttribute(string serviceName, string methodName)
        {
            MethodName = methodName;
            ServiceName = serviceName;
        }

        public string MethodName { get; private set; }
        public string ServiceName { get; private set; }
    }
}