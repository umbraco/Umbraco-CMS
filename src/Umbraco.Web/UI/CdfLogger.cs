using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.Logging;
using Umbraco.Core.Logging;
using ILogger = ClientDependency.Core.Logging.ILogger;

namespace Umbraco.Web.UI
{
    /// <summary>
    /// A logger for ClientDependency
    /// </summary>
    public class CdfLogger : ILogger
    {
        public void Debug(string msg)
        {
            LogHelper.Debug<CdfLogger>(msg);
        }

        public void Info(string msg)
        {
            LogHelper.Info<CdfLogger>(msg);
        }

        public void Warn(string msg)
        {
            LogHelper.Warn<CdfLogger>(msg);
        }

        public void Error(string msg, Exception ex)
        {
            LogHelper.Error<CdfLogger>(msg, ex);
        }

        public void Fatal(string msg, Exception ex)
        {
            LogHelper.Error<CdfLogger>(msg, ex);
        }
    }
}
