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

        public void Fatal<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Fatal(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Fatal<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Fatal(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Fatal<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
            => Fatal(reporting, exception, messageTemplate, new object[] { propertyValue0 });

        public void Error<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
         => Error(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Error<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        => Error(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Error<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        => Error(reporting, exception, messageTemplate, new object[] { propertyValue0 });

        public void Warn<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        => Warn(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Warn<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        => Warn(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Warn<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        => Warn(reporting, exception, messageTemplate, new object[] { propertyValue0 });

        public void Warn<T0>(Type reporting, string message, T0 propertyValue0)
        => Warn(reporting, message, new object[] { propertyValue0 });

        public void Info<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        => Info(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Info<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        => Info(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Info<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
        => Info(reporting, messageTemplate, new object[] { propertyValue0 });

        public void Debug<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        => Debug(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Debug<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        => Debug(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Debug<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
        => Debug(reporting, messageTemplate, new object[] { propertyValue0 });

        public void Verbose<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        => Verbose(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Verbose<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        => Verbose(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Verbose<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
        => Verbose(reporting, messageTemplate, new object[] { propertyValue0 });
    }
}
