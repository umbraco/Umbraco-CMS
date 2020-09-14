using System;

namespace Umbraco.Core.Logging
{
    public class NullLogger : ILogger
    {
        public bool IsEnabled(Type reporting, LogLevel level) => false;

        public void Fatal(Type reporting, Exception exception, string message)
        {

        }

        public void Fatal(Type reporting, Exception exception)
        {

        }

        public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogCritical(string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogError(Type reporting, Exception exception, string message)
        {

        }

        public void LogError(Type reporting, Exception exception)
        {

        }

        public void LogError(Type reporting, string message)
        {

        }

        public void LogError(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogError(Type reporting, string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogWarning(Type reporting, string message)
        {

        }

        public void LogWarning(Type reporting, string messageTemplate, params object[] propertyValues)
        {

        }

        public void LogWarning(Type reporting, Exception exception, string message)
        {

        }

        public void LogWarning(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
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
