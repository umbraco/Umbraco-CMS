using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A custom ActionMethodSelector which will ensure that the RenderMvcController.Index(ContentModel model) action will be executed
    /// if the
    /// </summary>
    internal class RenderIndexActionSelectorAttribute : ActionMethodSelectorAttribute
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<ControllerActionDescriptor>> _controllerActionsCache = new ConcurrentDictionary<Type, IEnumerable<ControllerActionDescriptor>>();

        /// <summary>
        /// Determines whether the action method selection is valid for the specified controller context.
        /// </summary>
        /// <returns>
        /// true if the action method selection is valid for the specified controller context; otherwise, false.
        /// </returns>
        /// <param name="routeContext">The route context.</param>
        /// <param name="action">Information about the action method.</param>
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (action is ControllerActionDescriptor controllerAction)
            {
                var currType = controllerAction.ControllerTypeInfo.UnderlyingSystemType;
                var baseType = controllerAction.ControllerTypeInfo.BaseType;

                //It's the same type, so this must be the Index action to use
                if (currType == baseType) return true;

                 var actions = _controllerActionsCache.GetOrAdd(currType, type =>
                 {
                     var actionDescriptors = routeContext.HttpContext.RequestServices
                         .GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items
                         .Where(x=>x is ControllerActionDescriptor).Cast<ControllerActionDescriptor>()
                         .Where(x => x.ControllerTypeInfo == controllerAction.ControllerTypeInfo);

                     return actionDescriptors;
                 });

                //If there are more than one Index action for this controller, then
                // this base class one should not be matched
                return actions.Count(x => x.ActionName == "Index") <= 1;
            }

            return false;

        }
    }
}
