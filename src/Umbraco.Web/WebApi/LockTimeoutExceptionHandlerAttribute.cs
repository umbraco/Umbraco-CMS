using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Umbraco.Core.Exceptions;

namespace Umbraco.Web.WebApi
{

    public class LockTimeoutExceptionHandlerAttribute : FilterAttribute, IExceptionFilter
    {
        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext.Exception is LockTimeoutException timeoutException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.Conflict, timeoutException.Reason);
            }
            return Task.CompletedTask;
        }
    }
}
