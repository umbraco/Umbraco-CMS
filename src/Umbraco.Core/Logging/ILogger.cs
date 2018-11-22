using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Interface for logging service.
    /// </summary>
    public interface ILogger
    {
        void Error(Type callingType, string message, Exception exception);

        void Warn(Type callingType, string message, params Func<object>[] formatItems);
       
        void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems);

        void Info(Type callingType, Func<string> generateMessage);

        void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems);

        void Debug(Type callingType, Func<string> generateMessage);

        void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems);
    }
}