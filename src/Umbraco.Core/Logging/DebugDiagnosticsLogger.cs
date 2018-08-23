using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Implements <see cref="ILogger"/> on top of <see cref="System.Diagnostics"/>.
    /// </summary>
    public class DebugDiagnosticsLogger : ILogger
    {
        /// <inheritdoc/>
        public void Error(Type reporting, Exception exception, string message)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, Exception exception, string messageTemplate, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(messageTemplate, args) + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, string messageTemplate, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(messageTemplate, args);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string format)
        {
            System.Diagnostics.Debug.WriteLine(format, reporting.FullName);
        }
        
        /// <inheritdoc/>
        public void Warn(Type reporting, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string message)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format + Environment.NewLine + exception, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Verbose(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Verbose(Type reporting, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args), reporting.FullName);
        }

        public void Fatal(Type reporting, Exception exception, string message)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, reporting.FullName);
        }

        public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(messageTemplate, args) + Environment.NewLine + exception, reporting.FullName);
        }
    }
}
