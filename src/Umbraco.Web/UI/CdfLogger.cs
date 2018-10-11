using System;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;
using ICdfLogger = ClientDependency.Core.Logging.ILogger;
using ICoreLogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Web.UI
{
    /// <summary>
    /// A logger for ClientDependency
    /// </summary>
    public class CdfLogger : ICdfLogger
    {
        private readonly ICoreLogger _logger;

        // Client Dependency doesn't know how to inject
        public CdfLogger(/*ICoreLogger logger*/)
        {
            _logger = Current.Logger;
        }

        public void Debug(string msg)
        {
            _logger.Debug<CdfLogger>(msg);
        }

        public void Info(string msg)
        {
            _logger.Info<CdfLogger>(msg);
        }

        public void Warn(string msg)
        {
            _logger.Warn<CdfLogger>(msg);
        }

        public void Error(string msg, Exception ex)
        {
            _logger.Error<CdfLogger>(ex, msg);
        }

        public void Fatal(string msg, Exception ex)
        {
            _logger.Error<CdfLogger>(ex, msg);
        }
    }
}
