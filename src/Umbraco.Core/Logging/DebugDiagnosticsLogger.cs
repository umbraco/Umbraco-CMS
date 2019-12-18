using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Implements <see cref="ILogger"/> on top of <see cref="System.Diagnostics"/>.
    /// </summary>
    public class DebugDiagnosticsLogger : ILogger
    {
        public bool IsEnabled(Type reporting, LogLevel level)
            => true;

        /// <inheritdoc/>
        public void Fatal(Type reporting, Exception exception, string message)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(MessageTemplates.Render(messageTemplate, propertyValues) + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Fatal(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(messageTemplate, propertyValues);
        }

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
        public void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(MessageTemplates.Render(messageTemplate, propertyValues) + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Error(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, string message, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(MessageTemplates.Render(message, propertyValues), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string message)
        {
            System.Diagnostics.Debug.WriteLine(message + Environment.NewLine + exception, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Warn(Type reporting, Exception exception, string message, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(MessageTemplates.Render(message + Environment.NewLine + exception, propertyValues), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Info(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(MessageTemplates.Render(messageTemplate, propertyValues), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Debug(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(MessageTemplates.Render(messageTemplate, propertyValues), reporting.FullName);
        }

        /// <inheritdoc/>
        public void Verbose(Type reporting, string message)
        {
            System.Diagnostics.Debug.WriteLine(message, reporting.FullName);
        }

        /// <inheritdoc/>
        public void Verbose(Type reporting, string messageTemplate, params object[] propertyValues)
        {
            System.Diagnostics.Debug.WriteLine(MessageTemplates.Render(messageTemplate, propertyValues), reporting.FullName);
        }
    }
}
