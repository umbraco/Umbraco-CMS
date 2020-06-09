
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Web.Common.Formatters;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// Applying this attribute to any controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
    /// </summary>
    public class AngularJsonOnlyConfigurationAttribute : TypeFilterAttribute
    {
        public AngularJsonOnlyConfigurationAttribute() : base(typeof(AngularJsonOnlyConfigurationFilter))
        {
        }

        private class AngularJsonOnlyConfigurationFilter :  IResultFilter
        {
            private readonly IOptions<MvcNewtonsoftJsonOptions> _mvcNewtonsoftJsonOptions;
            private readonly ArrayPool<char> _arrayPool;
            private readonly IOptions<MvcOptions> _options;

            public AngularJsonOnlyConfigurationFilter(IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions, ArrayPool<char> arrayPool, IOptions<MvcOptions> options)
            {
                _mvcNewtonsoftJsonOptions = mvcNewtonsoftJsonOptions;
                _arrayPool = arrayPool;
                _options = options;
            }

            public void OnResultExecuted(ResultExecutedContext context)
            {

            }

            public void OnResultExecuting(ResultExecutingContext context)
            {
                if (context.Result is ObjectResult objectResult)
                {
                    objectResult.Formatters.Clear();
                    objectResult.Formatters.Add(new AngularJsonMediaTypeFormatter(_mvcNewtonsoftJsonOptions.Value.SerializerSettings, _arrayPool, _options.Value));
                }
            }
        }
    }


}
