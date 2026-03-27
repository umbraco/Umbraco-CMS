using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods to assist with loading and managing infrastructure-related types in Umbraco.
/// </summary>
public static class InfrastuctureTypeLoaderExtensions
{
    /// <summary>
    ///     Gets all types implementing <see cref="PackageMigrationPlan" />
    /// </summary>
    /// <param name="mgr"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetPackageMigrationPlans(this TypeLoader mgr) =>
        mgr.GetTypes<PackageMigrationPlan>();
}
