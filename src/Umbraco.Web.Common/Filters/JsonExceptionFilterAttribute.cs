using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Web.Common.Filters;

public class JsonExceptionFilterAttribute : TypeFilterAttribute
{
    public JsonExceptionFilterAttribute()
        : base(typeof(JsonExceptionFilter))
    {
    }

    private class JsonExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public JsonExceptionFilter(IHostingEnvironment hostingEnvironment) => _hostingEnvironment = hostingEnvironment;

        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                filterContext.Result = new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ContentType = MediaTypeNames.Application.Json,
                    Content = JsonConvert.SerializeObject(
                        GetModel(filterContext.Exception),
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                };
                filterContext.ExceptionHandled = true;
            }
        }

        private object GetModel(Exception ex)
        {
            var error = new ExceptionViewModel { ExceptionMessage = ex.Message };

            if (_hostingEnvironment.IsDebugMode)
            {
                error.ExceptionType = ex.GetType();
                error.StackTrace = ex.StackTrace;
            }

            return error;
        }
    }
}
