using System;

namespace Umbraco.Core.Logging
{
    public class NullLogger : ILogger
    {
        public bool IsEnabled(Type reporting, LogLevel level) => false;

        public void Fatal(Type reporting, Exception exception)
        {

        }

        public void LogCritical(Exception exception, string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogCritical(string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogError(Type reporting, Exception exception)
        {

        }

        public void LogError(string message, params object[] propertyValues)
        {

        }

        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogWarning(string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {

        }

        public void Info(Type reporting, string message)
        {

        }

        public void Info(Type reporting, string messageTemplate, params object[] propertyValues)
        {

        }

        public void Debug(Type reporting, string message)
        {

        }

        public void Debug(Type reporting, string messageTemplate, params object[] propertyValues)
        {

        }

        public void Verbose(Type reporting, string message)
        {

        }

        public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues)
        {

        }
    }
}
