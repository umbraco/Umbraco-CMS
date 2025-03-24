using Umbraco.Cms.Api.Management.Controllers;

namespace Umbraco.Cms.Api.Management.Extensions;

public static class TypeExtensions
{
    public static bool IsManagementApiController(this Type type)
        => typeof(ManagementApiControllerBase).IsAssignableFrom(type);

    public static bool IsManagementApiViewModel(this Type type)
        => type.Namespace?.StartsWith("Umbraco.Cms.Api.Management.ViewModels") is true;
}
