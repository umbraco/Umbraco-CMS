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

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// Applying this attribute to any controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
    /// </summary>
    public class AngularJsonOnlyConfigurationAttribute : ActionFilterAttribute
    {
        private readonly IOptions<MvcNewtonsoftJsonOptions> _mvcNewtonsoftJsonOptions;
        private readonly ArrayPool<char> _arrayPool;
        private readonly IOptions<MvcOptions> _options;

        public AngularJsonOnlyConfigurationAttribute(IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions, ArrayPool<char> arrayPool, IOptions<MvcOptions> options)
        {
            _mvcNewtonsoftJsonOptions = mvcNewtonsoftJsonOptions;
            _arrayPool = arrayPool;
            _options = options;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Clear();
                objectResult.Formatters.Add(new AngularJsonMediaTypeFormatter(_mvcNewtonsoftJsonOptions.Value.SerializerSettings, _arrayPool, _options.Value));
            }

            base.OnResultExecuting(context);
        }
    }

}
