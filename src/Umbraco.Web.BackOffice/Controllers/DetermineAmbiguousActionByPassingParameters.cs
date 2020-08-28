using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.ModelBinders;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// This method determine ambiguous controller actions by making a tryparse of the string (from request) into the type of the argument.
    /// </summary>
    /// <remarks>
    /// By using this methods you are allowed to to have multiple controller actions named equally. E.g. GetById(Guid id), GetById(int id),...
    /// </remarks>
    public class DetermineAmbiguousActionByPassingParameters : ActionMethodSelectorAttribute
    {
        private string _requestBody = null;

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var parameters = action.Parameters;
            if (parameters.Any())
            {
                var canUse = true;
                foreach (var parameterDescriptor in parameters)
                {
                    var values = GetValue(parameterDescriptor, routeContext);

                    var type = parameterDescriptor.ParameterType;

                    if(typeof(IEnumerable).IsAssignableFrom(type) && typeof(string) != type)
                    {
                        type = type.GetElementType();
                    }

                    if (values is null )
                    {
                        canUse &= Nullable.GetUnderlyingType(type) != null;
                    }
                    else
                    {
                        foreach (var value in values)
                        {
                            if (type == typeof(Udi))
                            {
                                canUse &= UdiParser.TryParse(value.ToString(), out _);
                            }
                            else if (type == typeof(int))
                            {
                                canUse &= int.TryParse(value.ToString(), out _);
                            }
                            else if (type == typeof(Guid))
                            {
                                canUse &= Guid.TryParse(value.ToString(), out _);
                            }
                            else if (type == typeof(string))
                            {
                                canUse &= true;
                            }
                            else if (type == typeof(bool))
                            {
                                canUse &= bool.TryParse(value, out _);
                            }
                            else if (type == typeof(Direction))
                            {
                                canUse &= Enum<Direction>.TryParse(value, out _);
                            }
                            else
                            {
                                canUse &= true;
                            }
                        }
                    }

                }

                return canUse;
            }


            return true;
        }

        private IEnumerable<string> GetValue(ParameterDescriptor descriptor, RouteContext routeContext)
        {
            if (routeContext.HttpContext.Request.Query.ContainsKey(descriptor.Name))
            {
                return routeContext.HttpContext.Request.Query[descriptor.Name];
            }

            if (descriptor.BindingInfo.BinderType == typeof(FromJsonPathAttribute.JsonPathBinder))
            {
                // IMPORTANT: Ensure the requestBody can be read multiple times.
                routeContext.HttpContext.Request.EnableBuffering();

                // We need to use the asynchronous method here if synchronous IO is not allowed (it may or may not be, depending
                // on configuration in UmbracoBackOfficeServiceCollectionExtensions.AddUmbraco()).
                // We can't use async/await due to the need to override IsValidForRequest, which doesn't have an async override, so going with
                // this, which seems to be the least worst option for "sync to async" (https://stackoverflow.com/a/32429753/489433).
                var body = _requestBody ??= Task.Run(() => routeContext.HttpContext.Request.GetRawBodyStringAsync()).GetAwaiter().GetResult();

                var jToken = JsonConvert.DeserializeObject<JToken>(body);

                return jToken[descriptor.Name].Values<string>();
            }

            if (routeContext.HttpContext.Request.Method.InvariantEquals(HttpMethod.Post.ToString()) && routeContext.HttpContext.Request.Form.ContainsKey(descriptor.Name))
            {
                return routeContext.HttpContext.Request.Form[descriptor.Name];
            }

            return null;

        }
    }

}


