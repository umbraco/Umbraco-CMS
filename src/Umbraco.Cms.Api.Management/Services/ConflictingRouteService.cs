using System.Reflection;
using System.Runtime.InteropServices;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Api.Management.Services;

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
        var controllers = _typeLoader.GetTypes<ManagementApiControllerBase>().ToList();
        foreach (Type controller in CollectionsMarshal.AsSpan(controllers))
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
