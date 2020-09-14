using System;

namespace Umbraco.Core.Logging
{
    public class ConsoleLogger<T> : ILogger
    {
        private readonly IMessageTemplates _messageTemplates;

        public ConsoleLogger(IMessageTemplates messageTemplates)
        {
            _messageTemplates = messageTemplates;
        }

        public bool IsEnabled(Type reporting, LogLevel level)
            => true;

        public void Fatal(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Fatal(Type reporting, Exception exception)
        {
            Console.WriteLine("FATAL {0}", reporting.Name);
            Console.WriteLine(exception);
        }

        public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, _messageTemplates.Render(messageTemplate, propertyValues));
            Console.WriteLine(exception);
        }

        public void LogCritical(string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("FATAL {0} - {1}", typeof(T).Name, _messageTemplates.Render(messageTemplate, propertyValues));
        }

        public void LogError(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void LogError(Type reporting, Exception exception)
        {
            Console.WriteLine("ERROR {0}", reporting.Name);
            Console.WriteLine(exception);
        }

        public void LogError(Type reporting, string message)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, message);
        }

        public void LogError(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, _messageTemplates.Render(messageTemplate, propertyValues));
            Console.WriteLine(exception);
        }

        public void LogError(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, _messageTemplates.Render(messageTemplate, propertyValues));
        }

        public void LogWarning(Type reporting, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
        }

        public void LogWarning(Type reporting, string message, params object[] propertyValues)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, _messageTemplates.Render(message, propertyValues));
        }

        public void LogWarning(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void LogWarning(Type reporting, Exception exception, string message, params object[] propertyValues)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, _messageTemplates.Render(message, propertyValues));
            Console.WriteLine(exception);
        }

        public void Info(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, _messageTemplates.Render(messageTemplate, propertyValues));
        }

        public void Info(Type reporting, string message)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, message);
        }

        public void Debug(Type reporting, string message)
        {
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, message);
        }

        public void Debug(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, _messageTemplates.Render(messageTemplate, propertyValues));
        }

        public void Verbose(Type reporting, string message)
        {
            Console.WriteLine("VERBOSE {0} - {1}", reporting.Name, message);
        }

        public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("VERBOSE {0} - {1}", reporting.Name, _messageTemplates.Render(messageTemplate, propertyValues));
        }
    }
}
