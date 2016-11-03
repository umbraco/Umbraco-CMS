using System;
using System.Linq;
using Umbraco.Core.Logging;

// ReSharper disable LocalizableElement

namespace Umbraco.Tests.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Error(Type reporting, string message, Exception exception = null)
        {
            WriteLine("WARN", reporting, message);
            if (exception != null)
                Console.WriteLine(exception);
        }

        public void Warn(Type reporting, string message)
        {
            WriteLine("WARN", reporting, message);
        }

        public void Warn(Type reporting, Func<string> messageBuilder)
        {
            WriteLine("WARN", reporting, messageBuilder());
        }

        public void Warn(Type reporting, string format, params object[] args)
        {
            WriteLine("WARN", reporting, string.Format(format, args));
        }

        public void Warn(Type reporting, string format, params Func<object>[] args)
        {
            WriteLine("WARN", reporting, string.Format(format, args.Select(x => x())));
        }

        public void Warn(Type reporting, Exception exception, string message)
        {
            Warn(reporting, message);
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, Exception exception, Func<string> messageBuilder)
        {
            Warn(reporting, messageBuilder);
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, Exception exception, string format, params object[] args)
        {
            Warn(reporting, format, args);
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, Exception exception, string format, params Func<object>[] args)
        {
            Warn(reporting, format, args);
            Console.WriteLine(exception);
        }

        public void Info(Type reporting, string message)
        {
            WriteLine("INFO", reporting, message);
        }

        public void Info(Type reporting, Func<string> messageBuilder)
        {
            WriteLine("INFO", reporting, messageBuilder());
        }

        public void Info(Type reporting, string format, params object[] args)
        {
            WriteLine("INFO", reporting, string.Format(format, args));
        }

        public void Info(Type reporting, string format, params Func<object>[] args)
        {
            WriteLine("INFO", reporting, string.Format(format, args.Select(x => x())));
        }

        public void Debug(Type reporting, string message)
        {
            WriteLine("DEBUG", reporting, message);
        }

        public void Debug(Type reporting, Func<string> messageBuilder)
        {
            WriteLine("DEBUG", reporting, messageBuilder());
        }

        public void Debug(Type reporting, string format, params object[] args)
        {
            WriteLine("DEBUG", reporting, string.Format(format, args));
        }

        public void Debug(Type reporting, string format, params Func<object>[] args)
        {
            WriteLine("DEBUG", reporting, string.Format(format, args.Select(x => x())));
        }

        private static void WriteLine(string level, Type reporting, string message)
        {
            Console.WriteLine("{0} {1} - {2}", level, reporting.Name, message);
        }
    }
}
