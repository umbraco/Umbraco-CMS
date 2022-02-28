using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Web.Common.Formatters;

namespace Umbraco.Cms.Web.Common.Filters
{
    public class AllowHttpJsonConfigrationAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// This filter overwrites  AngularJsonOnlyConfigurationAttribute and get the api back to its defualt behavior 
        /// </summary>
        public AllowHttpJsonConfigrationAttribute() : base(typeof(AllowJsonXHRConfigrationFilter))
        {
            Order = 2; // this value must be more than the AngularJsonOnlyConfigurationAttribute on order to overwrtie it 
        }

        private class AllowJsonXHRConfigrationFilter : IResultFilter
        {
            public void OnResultExecuted(ResultExecutedContext context)
            {
            }

            public void OnResultExecuting(ResultExecutingContext context)
            {
                if (context.Result is ObjectResult objectResult)
                {
                    objectResult.Formatters.RemoveType<AngularJsonMediaTypeFormatter>();
                }
            }
        }
    }
}
