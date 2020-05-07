using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Web.Common.Formatters;

namespace Umbraco.Web.Common.Attributes
{
    /// <summary>
    /// Applying this attribute to any controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
    /// </summary>
    public class AngularJsonOnlyConfigurationAttribute : ActionFilterAttribute
    {

        public override void OnResultExecuting(ResultExecutingContext context)
        {

            var mvcNewtonsoftJsonOptions = context.HttpContext.RequestServices.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
            var arrayPool = context.HttpContext.RequestServices.GetService<ArrayPool<char>>();
            var mvcOptions = context.HttpContext.RequestServices.GetService<IOptions<MvcOptions>>();


            if (context.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Clear();
                objectResult.Formatters.Add(new AngularJsonMediaTypeFormatter(mvcNewtonsoftJsonOptions.Value.SerializerSettings, arrayPool, mvcOptions.Value));
            }

            base.OnResultExecuting(context);
        }
    }
}
