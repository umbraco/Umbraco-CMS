using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Umbraco.Cms.Web.Common.Filters;

namespace Umbraco.Cms.Web.Common.ApplicationModels;

/// <summary>
///     Adds the <see cref="UmbracoVirtualPageFilterAttribute" /> as a convention
/// </summary>
public class VirtualPageConvention : IActionModelConvention
{
    /// <inheritdoc />
    public void Apply(ActionModel action) => action.Filters.Add(new UmbracoVirtualPageFilterAttribute());
}
