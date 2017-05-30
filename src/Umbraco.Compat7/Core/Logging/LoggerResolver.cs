using System;
using CoreCurrent = Umbraco.Core.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Logging
{
    public class LoggerResolver
    {
        private LoggerResolver()
        { }

        public static bool HasCurrent => true;

        public static LoggerResolver Current { get; }
            = new LoggerResolver();

        public ILogger Logger => CoreCurrent.Logger;

        public void SetLogger(ILogger logger)
        {
            throw new NotSupportedException("The logger is determined by the UmbracoApplicationBase and cannot be modified afterwards."
                + " To use a different logger, override the UmbracoApplicationBase (global.asax) class.");
        }
    }
}
