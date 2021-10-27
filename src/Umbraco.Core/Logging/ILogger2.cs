using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Logging
{
    public interface ILogger2 : ILogger
    {
        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Fatal<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Fatal<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Fatal<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Error<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Error<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Error<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Warn<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Warn<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Warn<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Warn<T0>(Type reporting, string message, T0 propertyValue0);

        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Info<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Info<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Info<T0>(Type reporting, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Debug<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Debug<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Debug<T0>(Type reporting, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Verbose<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Verbose<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Verbose<T0>(Type reporting, string messageTemplate, T0 propertyValue0);


        /// <summary>
        /// Logs a error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Error<T0>(Type reporting, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs a error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Error<T0,T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1);

        /// <summary>
        /// Logs a error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Error<T0, T1,T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Warn<T, T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Warn<T, T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Warn<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Warn<T0, T1>(Type reporting,  string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    }
}
