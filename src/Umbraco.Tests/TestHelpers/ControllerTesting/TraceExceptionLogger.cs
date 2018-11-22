using System.Diagnostics;
using System.Web.Http.ExceptionHandling;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    /// <summary>
    /// Traces any errors for WebApi to the output window
    /// </summary>
    public class TraceExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            Trace.TraceError(context.ExceptionContext.Exception.ToString());
        }
    }
}