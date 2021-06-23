using Examine.Logging;
using Umbraco.Core.Logging;
using LogLevel = Examine.Logging.LogLevel;

namespace Umbraco.Examine
{
    public class UmbracoExamineLoggingService : ILoggingService
    {
        private ILogger Logger;

        public UmbracoExamineLoggingService(ILogger logger)
        {
            Logger = logger;
        }

        public void Log(LogEntry logEntry)
        {
            switch (logEntry.Level)
            {
                case LogLevel.Debug:
                    Logger.Debug<UmbracoExamineLoggingService>(logEntry.Message);
                    break;
                case LogLevel.Error:
                    Logger.Error<UmbracoExamineLoggingService>(logEntry.Exception, logEntry.Message);
                    break;
                case LogLevel.Info:
                    Logger.Info<UmbracoExamineLoggingService>(logEntry.Message);
                    break;
                case LogLevel.Trace:
                    Logger.Verbose<UmbracoExamineLoggingService>(logEntry.Message);
                    break;
                case LogLevel.Warning:
                    Logger.Warn<UmbracoExamineLoggingService>(logEntry.Message);
                    break;
                case LogLevel.Fatal:
                    Logger.Fatal<UmbracoExamineLoggingService>(logEntry.Exception,logEntry.Message);
                    break;
            }
        }
    }
}
