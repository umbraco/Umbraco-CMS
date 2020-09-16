using System;
using Microsoft.Extensions.Logging;
using Umbraco.Web.Composing;
using ICdfLogger = ClientDependency.Core.Logging.ILogger;
using ICoreLogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Web
{
    /// <summary>
    /// A logger for ClientDependency
    /// </summary>
    public class CdfLogger : ICdfLogger
    {
        private readonly ILogger<object> _logger;

        // Client Dependency doesn't know how to inject
        public CdfLogger(/*ICoreLogger logger*/)
        {
            _logger = Current.Logger;
        }

        public void Debug(string msg)
        {
            _logger.LogDebug(msg);
        }

        public void Info(string msg)
        {
            _logger.LogInformation(msg);
        }

        public void Warn(string msg)
        {
            _logger.LogWarning(msg);
        }

        public void Error(string msg, Exception ex)
        {
            _logger.LogError(ex, msg);
        }

        public void Fatal(string msg, Exception ex)
        {
            _logger.LogError(ex, msg);
        }
    }
}
