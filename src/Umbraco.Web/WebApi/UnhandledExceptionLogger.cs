using System.Web.Http.ExceptionHandling;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Used to log unhandled exceptions in webapi controllers
    /// </summary>
    public class UnhandledExceptionLogger : ExceptionLogger
    {
        private readonly ILogger _logger;

        public UnhandledExceptionLogger()
            : this(Current.Logger)
        {
        }

        public UnhandledExceptionLogger(ILogger logger)
        {
            _logger = logger;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            if (context != null && context.Exception != null)
            {
                var requestUrl = context.ExceptionContext?.ControllerContext?.Request?.RequestUri?.AbsoluteUri;
                var controllerType = context.ExceptionContext?.ActionContext?.ControllerContext?.Controller?.GetType();

                _logger.Error<string>(controllerType, context.Exception, "Unhandled controller exception occurred for request '{RequestUrl}'", requestUrl);
            }
        }

    }
}
