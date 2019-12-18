using System;
using Microsoft.Owin.Logging;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging
{
    internal class OwinLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Creates a new ILogger instance of the given name.
        /// </summary>
        /// <param name="name"/>
        /// <returns/>
        public Microsoft.Owin.Logging.ILogger Create(string name)
        {
            return new OwinLogger(Current.Logger, new Lazy<Type>(() => Type.GetType(name) ?? typeof (OwinLogger)));
        }
    }
}
