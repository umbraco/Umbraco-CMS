using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.Common.ApplicationModels
{
    /// <summary>
    /// Adds the <see cref="UmbracoVirtualPageFilterAttribute"/> as a convention
    /// </summary>
    public class VirtualPageConvention : IActionModelConvention
    {
        /// <inheritdoc/>
        public void Apply(ActionModel action) => action.Filters.Add(new UmbracoVirtualPageFilterAttribute());
    }
}
