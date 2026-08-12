using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="TypeLoader" /> class.
/// </summary>
public static class TypeLoaderExtensions
{
    /// <summary>
    ///     Gets all types implementing <see cref="SurfaceController" />.
    /// </summary>
    internal static IEnumerable<Type> GetSurfaceControllers(this TypeLoader typeLoader)
        => typeLoader.GetTypes<SurfaceController>();
}
