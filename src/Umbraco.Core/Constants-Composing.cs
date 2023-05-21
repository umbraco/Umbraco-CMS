namespace Umbraco.Cms.Core;

/// <summary>
///     Defines constants.
/// </summary>
public static partial class Constants
{
    /// <summary>
    ///     Defines constants for composition.
    /// </summary>
    public static class Composing
    {
        public static readonly string[] UmbracoCoreAssemblyNames =
        {
            "Umbraco.Core", "Umbraco.Infrastructure", "Umbraco.PublishedCache.NuCache", "Umbraco.Examine.Lucene",
            "Umbraco.Web.Common", "Umbraco.Web.BackOffice", "Umbraco.Web.Website",
        };
    }
}
