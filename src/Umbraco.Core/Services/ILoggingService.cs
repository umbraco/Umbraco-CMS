using System;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Interface for logging service.
    /// </summary>
    public interface ILoggingService
    {
        void Error<T>(string message, Exception exception);
        void Error(Type callingType, string message, Exception exception);
        void Warn(Type callingType, string message, params Func<object>[] formatItems);
        void Warn(Type callingType, string message, bool showHttpTrace, params Func<object>[] formatItems);
        void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems);
        void WarnWithException(Type callingType, string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems);

        void Warn<T>(string message, params Func<object>[] formatItems);
        void Warn<T>(string message, bool showHttpTrace, params Func<object>[] formatItems);
        void WarnWithException<T>(string message, Exception e, params Func<object>[] formatItems);
        void WarnWithException<T>(string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems);

        void Info<T>(Func<string> generateMessage);

        void Info(Type callingType, Func<string> generateMessage);

        void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems);

        void Info<T>(string generateMessageFormat, params Func<object>[] formatItems);

        void Debug<T>(Func<string> generateMessage);

        void Debug(Type callingType, Func<string> generateMessage);

        void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems);

        void Debug<T>(string generateMessageFormat, params Func<object>[] formatItems);

        void Debug<T>(string generateMessageFormat, bool showHttpTrace, params Func<object>[] formatItems);
    }
}