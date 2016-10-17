namespace Umbraco.Core.Logging
{
    using System;
    using System.Runtime.CompilerServices;

    using ImageProcessor.Common.Exceptions;

    /// <summary>
    /// A logger for explicitly logging ImageProcessor exceptions.
    /// <remarks>
    /// Creating this logger is enough for ImageProcessor to find and replace its in-built debug logger
    /// without any additional configuration required. This class currently has to be public in order 
    /// to do so.
    /// </remarks>
    /// </summary>
    public sealed class ImageProcessorLogger : ImageProcessor.Common.Exceptions.ILogger
    {
        /// <summary>
        /// Logs the specified message as an error.
        /// </summary>
        /// <typeparam name="T">The type calling the logger.</typeparam>
        /// <param name="text">The message to log.</param>
        /// <param name="callerName">The property or method name calling the log.</param>
        /// <param name="lineNumber">The line number where the method is called.</param>
        public void Log<T>(string text, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = 0)
        {
            // Using LogHelper since the ImageProcessor logger expects a parameterless constructor.
            var message = string.Format("{0} {1} : {2}", callerName, lineNumber, text);
            LogHelper.Error<T>(string.Empty, new ImageProcessingException(message));
        }

        /// <summary>
        /// Logs the specified message as an error.
        /// </summary>
        /// <param name="type">The type calling the logger.</param>
        /// <param name="text">The message to log.</param>
        /// <param name="callerName">The property or method name calling the log.</param>
        /// <param name="lineNumber">The line number where the method is called.</param>
        public void Log(Type type, string text, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = 0)
        {
            // Using LogHelper since the ImageProcessor logger expects a parameterless constructor.
            var message = string.Format("{0} {1} : {2}", callerName, lineNumber, text);
            LogHelper.Error(type, string.Empty, new ImageProcessingException(message));
        }
    }
}
