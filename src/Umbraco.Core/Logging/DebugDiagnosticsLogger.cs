using System;
using System.Linq;

namespace Umbraco.Core.Logging
{
    internal class DebugDiagnosticsLogger : ILogger
    {
        public void Error(Type callingType, string message, Exception exception)
        {
            System.Diagnostics.Debug.Fail(callingType.ToString(), message + Environment.NewLine + exception);
        }

        public void Warn(Type callingType, string message, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.Fail(callingType.ToString(), string.Format(message, formatItems.Select(x => x()).ToArray()));
        }

        public void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.Fail(callingType.ToString(), string.Format(message + Environment.NewLine + e, formatItems.Select(x => x()).ToArray()));
        }

        public void Info(Type callingType, Func<string> generateMessage)
        {
            System.Diagnostics.Debug.WriteLine(callingType.ToString(), generateMessage());
        }

        public void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.WriteLine(type.ToString(), string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()));
        }

        public void Debug(Type callingType, Func<string> generateMessage)
        {
            System.Diagnostics.Debug.WriteLine(callingType.ToString(), generateMessage());
        }

        public void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            System.Diagnostics.Debug.WriteLine(type.ToString(), string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()));
        }
    }
}