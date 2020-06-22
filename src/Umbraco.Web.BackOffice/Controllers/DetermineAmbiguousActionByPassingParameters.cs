using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.ModelBinders;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.BackOffice.Controllers
{
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

                var body = _requestBody ??= routeContext.HttpContext.Request.GetRawBodyString();

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


