using System;
using System.Linq;
using System.Web;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Web;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Logging
{
    public static class LogHelper
    {
        #region Error
        /// <summary>
        /// Adds an error log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error<T>(string message, Exception exception)
        {
            Current.Logger.Error(typeof(T), message, exception);
        }

        public static void Error(Type callingType, string message, Exception exception)
        {
            Current.Logger.Error(callingType, message, exception);
        }

        #endregion

        #region Warn

        public static void Warn(Type callingType, string message, params Func<object>[] formatItems)
        {
            Current.Logger.Warn(callingType, () => string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()));
        }

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
        public static void Warn(Type callingType, string message, bool showHttpTrace, params Func<object>[] formatItems)
        {
            if (callingType == null) throw new ArgumentNullException(nameof(callingType));
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullOrEmptyException(nameof(message));

            if (showHttpTrace && HttpContext.Current != null)
            {
                HttpContext.Current.Trace.Warn(callingType.Name, string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()));
            }

            Current.Logger.Warn(callingType, () => string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()));
        }

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
        public static void WarnWithException(Type callingType, string message, Exception e, params Func<object>[] formatItems)
        {
            WarnWithException(callingType, message, false, e, formatItems);
        }

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
        public static void WarnWithException(Type callingType, string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (callingType == null) throw new ArgumentNullException(nameof(callingType));
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullOrEmptyException(nameof(message));

            if (showHttpTrace && HttpContext.Current != null)
            {
                HttpContext.Current.Trace.Warn(
                    callingType.Name,
                    string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()),
                    e);
            }

            Current.Logger.Warn(callingType, e, string.Format(message, formatItems.Select(x => x.Invoke()).ToArray()));
        }

        /// <summary>
        /// Adds a warn log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="formatItems"></param>
        public static void Warn<T>(string message, params Func<object>[] formatItems)
        {
            Warn(typeof(T), message, formatItems);
        }

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
        public static void Warn<T>(string message, bool showHttpTrace, params Func<object>[] formatItems)
        {
            Warn(typeof(T), message, showHttpTrace, formatItems);
        }

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
        public static void WarnWithException<T>(string message, Exception e, params Func<object>[] formatItems)
        {
            WarnWithException(typeof(T), message, e, formatItems);
        }

        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
        public static void WarnWithException<T>(string message, bool showHttpTrace, Exception e, params Func<object>[] formatItems)
        {
            WarnWithException(typeof(T), message, showHttpTrace, e, formatItems);
        }

        #endregion

        #region Info
        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateMessage">The delegate to generate a message.</param>
        /// <remarks></remarks>
        public static void Info<T>(Func<string> generateMessage)
        {
            Info(typeof(T), generateMessage);
        }

        /// <summary>
        /// Traces if tracing is enabled.
        /// </summary>
        /// <param name="callingType"></param>
        /// <param name="generateMessage"></param>
        public static void Info(Type callingType, Func<string> generateMessage)
        {
            Current.Logger.Info(callingType, generateMessage);
        }

        /// <summary>
        /// Traces if tracing is enabled.
        /// </summary>
        /// <param name="type">The type for the logging namespace.</param>
        /// <param name="generateMessageFormat">The message format.</param>
        /// <param name="formatItems">The format items.</param>
        public static void Info(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            Current.Logger.Info(type, string.Format(generateMessageFormat, formatItems.Select(x => x.Invoke()).ToArray()));
        }

        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateMessageFormat">The generate message format.</param>
        /// <param name="formatItems">The format items.</param>
        /// <remarks></remarks>
        public static void Info<T>(string generateMessageFormat, params Func<object>[] formatItems)
        {
            Info(typeof(T), generateMessageFormat, formatItems);
        }
        #endregion

        #region Debug
        /// <summary>
        /// Debugs a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateMessage">The delegate to generate a message.</param>
        /// <remarks></remarks>
        public static void Debug<T>(Func<string> generateMessage)
        {
            Debug(typeof(T), generateMessage);
        }

        /// <summary>
        /// Debugs if tracing is enabled.
        /// </summary>
        /// <param name="callingType"></param>
        /// <param name="generateMessage"></param>
        public static void Debug(Type callingType, Func<string> generateMessage)
        {
            Current.Logger.Debug(callingType, generateMessage);
        }

        /// <summary>
        /// Debugs if tracing is enabled.
        /// </summary>
        /// <param name="type">The type for the logging namespace.</param>
        /// <param name="generateMessageFormat">The message format.</param>
        /// <param name="formatItems">The format items.</param>
        public static void Debug(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            Current.Logger.Debug(type, string.Format(generateMessageFormat, formatItems.Select(x => x.Invoke()).ToArray()));
        }

        /// <summary>
        /// Debugs a message, only generating the message if debug is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateMessageFormat">The generate message format.</param>
        /// <param name="formatItems">The format items.</param>
        /// <remarks></remarks>
        public static void Debug<T>(string generateMessageFormat, params Func<object>[] formatItems)
        {
            Debug(typeof(T), generateMessageFormat, formatItems);
        }

        /// <summary>
        /// Debugs a message and also writes to the TraceContext specified, useful for when you would like the debug
        /// output also displayed in the Http trace output.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateMessageFormat"></param>
        /// <param name="showHttpTrace"></param>
        /// <param name="formatItems"></param>
        [Obsolete("Warnings with http trace should not be used. This method will be removed in future versions")]
        public static void Debug<T>(string generateMessageFormat, bool showHttpTrace, params Func<object>[] formatItems)
        {
            if (showHttpTrace && HttpContext.Current != null)
            {
                HttpContext.Current.Trace.Write(
                    typeof(T).Name,
                    string.Format(generateMessageFormat, formatItems.Select(x => x()).ToArray()));
            }
            Debug(typeof(T), generateMessageFormat, formatItems);
        }

        #endregion

    }
}
