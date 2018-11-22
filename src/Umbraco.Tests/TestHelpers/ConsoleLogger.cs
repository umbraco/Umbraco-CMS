using System;
using System.Linq;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.TestHelpers
{
    public class ConsoleLogger : ILogger
    {
        public void Error(Type callingType, string message, Exception exception)
        {
            Console.WriteLine("INFO {0} - {1}", callingType.Name, message);
            Console.WriteLine(exception);
        }

        public void Warn(Type callingType, string message, params Func<object>[] formatItems)
        {
            Console.WriteLine("WARN {0} - {1}", callingType.Name, string.Format(message, formatItems.Select(x => x()).ToArray()));
        }

        public void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
        {
            Console.WriteLine("WARN {0} - {1}", callingType.Name, string.Format(message, formatItems.Select(x => x()).ToArray()));
            Console.WriteLine(e);
        }

        public void Info(Type callingType, Func<string> generateMessage)
        {
            Console.WriteLine("INFO {0} - {1}", callingType.Name, generateMessage());
        }

        public void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            Console.WriteLine("INFO {0} - {1}", type.Name, string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()));
        }

        public void Debug(Type callingType, Func<string> generateMessage)
        {
            Console.WriteLine("DEBUG {0} - {1}", callingType.Name, generateMessage());
        }

        public void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            Console.WriteLine("DEBUG {0} - {1}", type.Name, string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()));
        }
    }
}
