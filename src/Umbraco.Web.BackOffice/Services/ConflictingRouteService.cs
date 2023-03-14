using System.Reflection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.BackOffice.Services;

public class ConflictingRouteService : IConflictingRouteService
{
    private readonly TypeLoader _typeLoader;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConflictingRouteService" /> class.
    /// </summary>
    public ConflictingRouteService(TypeLoader typeLoader) => _typeLoader = typeLoader;

    /// <inheritdoc />
    public bool HasConflictingRoutes(out string controllerName)
    {
        var controllers = _typeLoader.GetTypes<UmbracoApiControllerBase>().ToList();
        foreach (Type controller in controllers)
        {
            Type[] potentialConflicting = controllers.Where(x => x.Name == controller.Name).ToArray();
            if (potentialConflicting.Length > 1)
            {
                //If we have any with same controller name and located in the same area, then it is a confict.
                var conflicting = potentialConflicting
                    .Select(x => x.GetCustomAttribute<PluginControllerAttribute>())
                    .GroupBy(x => x?.AreaName)
                    .Any(x => x?.Count() > 1);

                if (conflicting)
                {
                    controllerName = controller.Name;
                    return true;
                }
            }
        }

        controllerName = string.Empty;
        return false;
    }
}
