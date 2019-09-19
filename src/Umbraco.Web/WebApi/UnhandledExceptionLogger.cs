using System.Web.Http.ExceptionHandling;
using Umbraco.Core;
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
            : this(ApplicationContext.Current.ProfilingLogger.Logger)
        {
        }

        public UnhandledExceptionLogger(ILogger logger)
        {
            _logger = logger;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            if (context != null && context.ExceptionContext != null
                && context.ExceptionContext.ActionContext != null && context.ExceptionContext.ActionContext.ControllerContext != null
                && context.ExceptionContext.ActionContext.ControllerContext.Controller != null
                && context.Exception != null)
            {
                _logger.Error(context.ExceptionContext.ActionContext.ControllerContext.Controller.GetType(), "Unhandled controller exception occurred", context.Exception);
            }            
        }
        
    }
}