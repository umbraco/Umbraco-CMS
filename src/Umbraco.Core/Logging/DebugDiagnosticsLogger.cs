using System;
using System.Linq;

namespace Umbraco.Core.Logging
{
    internal class DebugDiagnosticsLogger : ILogger
    {
        public void Error(Type callingType, string message, Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, callingType.ToString());
        }

        public void Warn(Type callingType, string message, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(message, formatItems.Select(x => x()).ToArray()), callingType.ToString());
        }

        public void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(message + Environment.NewLine + e, formatItems.Select(x => x()).ToArray()), callingType.ToString());
        }

        public void Info(Type callingType, Func<string> generateMessage)
        {
            System.Diagnostics.Debug.WriteLine(generateMessage(), callingType.ToString());
        }

        public void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()), type.ToString());
        }

        public void Debug(Type callingType, Func<string> generateMessage)
        {
            System.Diagnostics.Debug.WriteLine(generateMessage(), callingType.ToString());
        }

        public void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()), type.ToString());
        }
    }
}