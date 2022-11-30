using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ApplicationModels;

/// <summary>
///     Applies the <see cref="VirtualPageConvention" /> to any action on a controller that is
///     <see cref="IVirtualPageController" />
/// </summary>
public class VirtualPageApplicationModelProvider : IApplicationModelProvider
{
    private readonly List<IActionModelConvention> _actionModelConventions = new() { new VirtualPageConvention() };

    /// <inheritdoc />
    /// <summary>
    ///     Will execute after <see cref="DefaultApplicationModelProvider" />
    /// </summary>
    public int Order => 0;

    /// <inheritdoc />
    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
    }

    /// <inheritdoc />
    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (ControllerModel controller in context.Result.Controllers)
        {
            if (!IsVirtualPageController(controller))
            {
                continue;
            }

            foreach (ActionModel action in controller.Actions.ToList())
            {
                if (action.ActionName == nameof(IVirtualPageController.FindContent)
                    && action.ActionMethod.ReturnType == typeof(IPublishedContent))
                {
                    // this is not an action, it's just the implementation of IVirtualPageController
                    controller.Actions.Remove(action);
                }
                else
                {
                    foreach (IActionModelConvention convention in _actionModelConventions)
                    {
                        convention.Apply(action);
                    }
                }
            }
        }
    }

    private bool IsVirtualPageController(ControllerModel controller)
        => controller.ControllerType.Implements<IVirtualPageController>();
}
