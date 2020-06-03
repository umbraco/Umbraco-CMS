using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Controllers
{
    public class DetermineAmbiguousActionByPassingParameters : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var parameters = action.Parameters;
            if (parameters.Any())
            {
                var canUse = true;
                foreach (var parameterDescriptor in parameters)
                {
                    var value = routeContext.HttpContext.Request.Query[parameterDescriptor.Name];

                    if (parameterDescriptor.ParameterType == typeof(Udi))
                    {
                        canUse &= UdiParser.TryParse(value, out _);
                    }
                    else if (parameterDescriptor.ParameterType == typeof(int))
                    {
                        canUse &= int.TryParse(value, out _);
                    }
                    else if (parameterDescriptor.ParameterType == typeof(Guid))
                    {
                        canUse &= Guid.TryParse(value, out _);
                    }
                    else if (parameterDescriptor.ParameterType == typeof(string))
                    {
                        canUse = true;
                    }
                    else
                    {
                        canUse = false;
                    }
                }

                return canUse;
            }


            return true;
        }
    }
}
