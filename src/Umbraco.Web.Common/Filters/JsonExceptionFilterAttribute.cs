using System;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.Common.Filters
{
    public class JsonExceptionFilterAttribute : TypeFilterAttribute
    {
        public JsonExceptionFilterAttribute() : base(typeof(JsonExceptionFilter))
        {
        }

        private class JsonExceptionFilter : IExceptionFilter
        {
            private readonly IHostingEnvironment _hostingEnvironment;

            public JsonExceptionFilter(IHostingEnvironment hostingEnvironment)
            {
                _hostingEnvironment = hostingEnvironment;
            }

            public void OnException(ExceptionContext filterContext)
            {
                if (filterContext.Exception != null)
                {
                    filterContext.HttpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                    filterContext.Result = new ContentResult
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        ContentType = MediaTypeNames.Application.Json,
                        Content = JsonConvert.SerializeObject(GetModel(filterContext.Exception),
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            })
                    };
                    filterContext.ExceptionHandled = true;
                }
            }

            private object GetModel(Exception ex)
            {
                object error;

                if (_hostingEnvironment.IsDebugMode)
                {
                    error = new
                    {
                        ExceptionMessage = ex.Message,
                        ExceptionType = ex.GetType(),
                        StackTrace = ex.StackTrace
                    };
                }
                else
                {
                    error = new
                    {
                        ExceptionMessage = ex.Message
                    };
                }

                return error;
            }
        }
    }
}
