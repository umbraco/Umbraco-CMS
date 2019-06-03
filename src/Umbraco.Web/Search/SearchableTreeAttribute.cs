using System;
using Umbraco.Core.Exceptions;

namespace Umbraco.Web.Search
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SearchableTreeAttribute : Attribute
    {
        public const int DefaultSortOrder = 1000;

        /// <summary>
        /// This constructor defines both the angular service and method name to use
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        public SearchableTreeAttribute(string serviceName, string methodName) : this(serviceName, methodName, DefaultSortOrder) { }

        /// <summary>
        /// This constructor defines both the angular service and method name to use and explicitly defines a sort order for the results
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="sortOrder"></param>
        public SearchableTreeAttribute(string serviceName, string methodName, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentNullOrEmptyException(nameof(serviceName));
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentNullOrEmptyException(nameof(methodName));
            MethodName = methodName;
            SortOrder = sortOrder;
            ServiceName = serviceName;
        }

        /// <summary>
        /// This constructor will assume that the method name equals `format(searchResult, appAlias, treeAlias)`
        /// </summary>
        /// <param name="serviceName"></param>
        public SearchableTreeAttribute(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentNullOrEmptyException(nameof(serviceName));
            MethodName = "";
            ServiceName = serviceName;
            SortOrder = DefaultSortOrder;
        }

        public string MethodName { get; }
        public string ServiceName { get; }
        public int SortOrder { get; }
    }
}
