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
        /// <inheritdoc/>
        public void Fatal<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0,
            T1 propertyValue1, T2 propertyValue2)
                => Fatal(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });
        /// <inheritdoc/>
        public void Fatal<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Fatal(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1 });
        /// <inheritdoc/>
        public void Fatal<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
            => Fatal(reporting, exception, messageTemplate, new object[] { propertyValue0 });
        /// <inheritdoc/>
        public void Error<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0,
            T1 propertyValue1, T2 propertyValue2)
                => Error(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });
        /// <inheritdoc/>
        public void Error<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Error(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1 });
        /// <inheritdoc/>
        public void Error<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
            => Error(reporting, exception, messageTemplate, new object[] { propertyValue0 });
        /// <inheritdoc/>
        public void Warn<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0,
            T1 propertyValue1, T2 propertyValue2)
                => Warn(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1,propertyValue2 });
        /// <inheritdoc/>
        public void Warn<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
                => Warn(reporting, exception, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Warn<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
            => Warn(reporting, exception, messageTemplate, new object[] { propertyValue0 });

        public void Warn<T0>(Type reporting, string message, T0 propertyValue0)
            => Warn(reporting, message, new object[] { propertyValue0 });

        public void Info<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Info(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Info<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Info(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Info<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
            => Info(reporting, messageTemplate, new object[] { propertyValue0 });

        public void Debug<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Debug(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1, propertyValue2 });

        public void Debug<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Debug(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Debug<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
            => Debug(reporting, messageTemplate, new object[] { propertyValue0 });

        public void Verbose<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
            => Verbose(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1,propertyValue2 });

        public void Verbose<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
            => Verbose(reporting, messageTemplate, new object[] { propertyValue0, propertyValue1 });

        public void Verbose<T0>(Type reporting, string messageTemplate, T0 propertyValue0)
            => Verbose(reporting, messageTemplate, new object[] { propertyValue0 });
    }
}
