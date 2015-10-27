using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Logging;

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
            return new OwinLogger(
                LoggerResolver.HasCurrent ? LoggerResolver.Current.Logger : new DebugDiagnosticsLogger(),
                new Lazy<Type>(() => Type.GetType(name) ?? typeof (OwinLogger)));
        }
    }
}
