using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.Common.Controllers;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// Determines if a custom controller can hijack the current route
    /// </summary>
    public class HijackedRouteEvaluator
    {
        private readonly ILogger<HijackedRouteEvaluator> _logger;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private const string DefaultActionName = nameof(RenderController.Index);

        /// <summary>
        /// Initializes a new instance of the <see cref="HijackedRouteEvaluator"/> class.
        /// </summary>
        public HijackedRouteEvaluator(
            ILogger<HijackedRouteEvaluator> logger,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _logger = logger;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public HijackedRouteResult Evaluate(string controller, string action)
        {
            IReadOnlyList<ControllerActionDescriptor> candidates = FindControllerCandidates(controller, action, DefaultActionName);

            // check if there's a custom controller assigned, base on the document type alias.
            var customControllerCandidates = candidates.Where(x => x.ControllerName.InvariantEquals(controller)).ToList();

            // check if that custom controller exists
            if (customControllerCandidates.Count > 0)
            {
                ControllerActionDescriptor controllerDescriptor = customControllerCandidates[0];

                // ensure the controller is of type IRenderController and ControllerBase
                if (TypeHelper.IsTypeAssignableFrom<IRenderController>(controllerDescriptor.ControllerTypeInfo)
                    && TypeHelper.IsTypeAssignableFrom<ControllerBase>(controllerDescriptor.ControllerTypeInfo))
                {
                    // now check if the custom action matches
                    var customActionExists = action != null && customControllerCandidates.Any(x => x.ActionName.InvariantEquals(action));

                    // it's a hijacked route with a custom controller, so return the the values
                    return new HijackedRouteResult(
                        true,
                        controllerDescriptor.ControllerName,
                        controllerDescriptor.ControllerTypeInfo,
                        customActionExists ? action : DefaultActionName);
                }
                else
                {
                    _logger.LogWarning(
                        "The current Document Type {ContentTypeAlias} matches a locally declared controller of type {ControllerName}. Custom Controllers for Umbraco routing must implement '{UmbracoRenderController}' and inherit from '{UmbracoControllerBase}'.",
                        controller,
                        controllerDescriptor.ControllerTypeInfo.FullName,
                        typeof(IRenderController).FullName,
                        typeof(ControllerBase).FullName);

                    // we cannot route to this custom controller since it is not of the correct type so we'll continue with the defaults
                    // that have already been set above.
                }
            }

            return HijackedRouteResult.Failed();
        }

        /// <summary>
        /// Return a list of controller candidates that match the custom controller and action names
        /// </summary>
        private IReadOnlyList<ControllerActionDescriptor> FindControllerCandidates(string customControllerName, string customActionName, string defaultActionName)
        {
            var descriptors = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                .Cast<ControllerActionDescriptor>()
                .Where(x => x.ControllerName.InvariantEquals(customControllerName)
                        && (x.ActionName.InvariantEquals(defaultActionName) || (customActionName != null && x.ActionName.InvariantEquals(customActionName))))
                .ToList();

            return descriptors;
        }
    }
}
