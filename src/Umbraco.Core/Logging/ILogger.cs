using System;
using System.ComponentModel;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Defines the logging service.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        /// <param name="exception">An exception.</param>
        void Error(Type reporting, string message, Exception exception = null);

        // note: should we have more overloads for Error too?

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Warn(Type reporting, string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Warn(Type reporting, Func<string> messageBuilder);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation")]
        void Warn(Type reporting, string format, params object[] args);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation, if you want to use lazy generated strings use the overload with messageBuilder")]
        void Warn(Type reporting, string format, params Func<object>[] args);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        void Warn(Type reporting, Exception exception, string message);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Warn(Type reporting, Exception exception, Func<string> messageBuilder);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation")]
        void Warn(Type reporting, Exception exception, string format, params object[] args);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation, if you want to use lazy generated strings use the overload with messageBuilder")]
        void Warn(Type reporting, Exception exception, string format, params Func<object>[] args);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Info(Type reporting, string message);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Info(Type reporting, Func<string> messageBuilder);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation")]
        void Info(Type reporting, string format, params object[] args);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation, if you want to use lazy generated strings use the overload with messageBuilder")]
        void Info(Type reporting, string format, params Func<object>[] args);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Debug(Type reporting, string message);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Debug(Type reporting, Func<string> messageBuilder);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation")]
        void Debug(Type reporting, string format, params object[] args);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed, do not use it, if you want to use formatting do it with string interpolation, if you want to use lazy generated strings use the overload with messageBuilder")]
        void Debug(Type reporting, string format, params Func<object>[] args);
    }
}
