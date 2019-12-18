using System;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Represents the data required to proxy a request to a surface controller for posted data
    /// </summary>
    internal class PostedDataProxyInfo : RouteDefinition
    {
        public string Area { get; set; }
    }
}
