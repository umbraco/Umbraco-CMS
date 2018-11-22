using System;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.TestHelpers
{
    public class ConsoleLogger : ILogger
    {
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

        public void Fatal(Type reporting, string message)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, message);
        }

        public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, MessageTemplates.Render(messageTemplate, propertyValues));
            Console.WriteLine(exception);
        }

        public void Fatal(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, MessageTemplates.Render(messageTemplate, propertyValues));
        }

        public void Error(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Error(Type reporting, Exception exception)
        {
            Console.WriteLine("ERROR {0}", reporting.Name);
            Console.WriteLine(exception);
        }

        public void Error(Type reporting, string message)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, message);
        }

        public void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, MessageTemplates.Render(messageTemplate, propertyValues));
            Console.WriteLine(exception);
        }

        public void Error(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, MessageTemplates.Render(messageTemplate, propertyValues));
        }

        public void Warn(Type reporting, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
        }

        public void Warn(Type reporting, string message, params object[] propertyValues)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, MessageTemplates.Render(message, propertyValues));
        }

        public void Warn(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, Exception exception, string message, params object[] propertyValues)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, MessageTemplates.Render(message, propertyValues));
            Console.WriteLine(exception);
        }

        public void Info(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, MessageTemplates.Render(messageTemplate, propertyValues));
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
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, MessageTemplates.Render(messageTemplate, propertyValues));
        }

        public void Verbose(Type reporting, string message)
        {
            Console.WriteLine("VERBOSE {0} - {1}", reporting.Name, message);
        }

        public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            Console.WriteLine("VERBOSE {0} - {1}", reporting.Name, MessageTemplates.Render(messageTemplate, propertyValues));
        }
    }
}
