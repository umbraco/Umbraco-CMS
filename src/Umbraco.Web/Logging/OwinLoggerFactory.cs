using System;
using Microsoft.Owin.Logging;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web.Logging
{
    [UmbracoVolatile]
    public class OwinLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Creates a new ILogger instance of the given name.
        /// </summary>
        /// <param name="name"/>
        /// <returns/>
        public Microsoft.Owin.Logging.ILogger Create(string name)
        {
            return new OwinLogger(StaticApplicationLogging.Logger, new Lazy<Type>(() => Type.GetType(name) ?? typeof (OwinLogger)));
        }
    }
}
