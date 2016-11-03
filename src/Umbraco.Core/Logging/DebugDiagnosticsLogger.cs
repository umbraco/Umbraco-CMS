using System;
using System.Linq;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Implements <see cref="ILogger"/> on top of <see cref="System.Diagnostics"/>.
    /// </summary>
    public class DebugDiagnosticsLogger : ILogger
    {
        /// <inheritdoc/>
        public void Error(Type reporting, string message, Exception exception = null)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Func<string> messageBuilder)
        {
            System.Diagnostics.Debug.WriteLine(messageBuilder(), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string format, params Func<object>[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args.Select(x => x()).ToArray()), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string message)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, Func<string> messageBuilder)
        {
            System.Diagnostics.Debug.WriteLine(messageBuilder() + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format + Environment.NewLine + exception, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string format, params Func<object>[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format + Environment.NewLine + exception, args.Select(x => x()).ToArray()), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, Func<string> messageBuilder)
        {
            System.Diagnostics.Debug.WriteLine(messageBuilder(), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string format, params Func<object>[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args.Select(x => x()).ToArray()), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, Func<string> messageBuilder)
        {
            System.Diagnostics.Debug.WriteLine(messageBuilder(), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string format, params Func<object>[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args.Select(x => x()).ToArray()), reporting.FullName);
        }
    }
}