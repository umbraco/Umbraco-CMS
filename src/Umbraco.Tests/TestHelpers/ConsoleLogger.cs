using System;
using System.Linq;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.TestHelpers
{
    public class ConsoleLogger : ILogger
    {
        public void Error(Type reporting, string message, Exception exception)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Error(Type reporting, string messageTemplate, Exception exception = null, params object[] propertyValues)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, string.Format(messageTemplate, propertyValues));
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
        }

        public void Warn(Type reporting, Func<string> messageBuilder)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, messageBuilder());
        }

        public void Warn(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, string.Format(format, args));
        }

        public void Warn(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, Exception exception, Func<string> messageBuilder)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, messageBuilder());
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, Exception exception, string format, params object[] args)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, string.Format(format, args));
            Console.WriteLine(exception);
        }

        public void Info(Type reporting, Func<string> generateMessage)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, generateMessage());
        }

        public void Info(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, string.Format(format, args));
        }

        public void Info(Type reporting, string message)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, message);
        }

        public void Debug(Type reporting, string message)
        {
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, message);
        }

        public void Debug(Type reporting, Func<string> messageBuilder)
        {
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, messageBuilder());
        }

        public void Debug(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, string.Format(format, args));
        }
    }
}
