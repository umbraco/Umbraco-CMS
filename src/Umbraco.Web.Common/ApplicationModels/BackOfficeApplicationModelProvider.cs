using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.Common.ApplicationModels;

// TODO: This should just exist in the back office project

/// <summary>
///     An application model provider for all Umbraco Back Office controllers
/// </summary>
public class BackOfficeApplicationModelProvider : IApplicationModelProvider
{
    private readonly List<IActionModelConvention> _actionModelConventions = new()
    {
        new BackOfficeIdentityCultureConvention(),
    };

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
            if (!IsBackOfficeController(controller))
            {
                continue;
            }

            foreach (ActionModel action in controller.Actions)
            {
                foreach (IActionModelConvention convention in _actionModelConventions)
                {
                    convention.Apply(action);
                }
            }
        }
    }

    private bool IsBackOfficeController(ControllerModel controller)
        => controller.Attributes.OfType<IsBackOfficeAttribute>().Any();
}
