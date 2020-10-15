using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Web.Common.Exceptions;

namespace Umbraco.Web.Common.Filters
{
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is HttpResponseException exception)
            {
                context.Result = new ObjectResult(exception.Value)
                {
                    StatusCode = (int)exception.Status,
                };

                foreach (var (key,value) in exception.AdditionalHeaders)
                {
                    context.HttpContext.Response.Headers[key] = value;
                }

                context.ExceptionHandled = true;
            }
        }
    }
}
