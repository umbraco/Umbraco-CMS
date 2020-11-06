using System;
using Microsoft.Owin.Logging;
using Umbraco.Core;

namespace Umbraco.Web.Logging
{
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
