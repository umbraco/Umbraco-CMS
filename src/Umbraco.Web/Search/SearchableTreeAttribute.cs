using System;

namespace Umbraco.Web.Search
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SearchableTreeAttribute : Attribute
    {
        /// <summary>
        /// This constructor defines both the angular service and method name to use
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        public SearchableTreeAttribute(string serviceName, string methodName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("Value cannot be null or whitespace.", "serviceName");
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentException("Value cannot be null or whitespace.", "methodName");
            MethodName = methodName;
            ServiceName = serviceName;
        }

        /// <summary>
        /// This constructor will assume that the method name equals `format(searchResult, appAlias, treeAlias)`
        /// </summary>
        /// <param name="serviceName"></param>
        public SearchableTreeAttribute(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("Value cannot be null or whitespace.", "serviceName");
            MethodName = "";
            ServiceName = serviceName;
        }

        public string MethodName { get; private set; }
        public string ServiceName { get; private set; }
    }
}