using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.WebApi
{

    public class LockTimeoutExceptionHandlerAttribute : FilterAttribute, IExceptionFilter
    {
        private readonly ILocalizedTextService _localizedTextService;

        public LockTimeoutExceptionHandlerAttribute()
        {
            _localizedTextService = Current.Factory.GetInstance<ILocalizedTextService>();
        }

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext.Exception is LockTimeoutException timeoutException)
            {
                var reason = _localizedTextService.LocalizeLockReason(timeoutException.Reason);
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.Conflict, reason);
            }
            return Task.CompletedTask;
        }
    }
}
