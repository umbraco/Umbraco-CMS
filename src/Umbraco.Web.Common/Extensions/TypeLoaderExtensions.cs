using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Extensions;

public static class TypeLoaderExtensions
{
    /// <summary>
    ///     Gets all types implementing <see cref="UmbracoApiController" />.
    /// </summary>
    public static IEnumerable<Type> GetUmbracoApiControllers(this TypeLoader typeLoader)
        => typeLoader.GetTypes<UmbracoApiController>();
}
