namespace Umbraco.Cms.Web.Common.Hosting;

/// <summary>
///     Umbraco-specific settings for static files from the Umbraco.Cms.StaticAssets RCL.
/// </summary>
public interface IBackOfficePathGenerator
{
    /// <summary>
    ///     Gets the virtual path of the BackOffice.
    /// </summary>
    string BackOfficePath { get; }

    /// <summary>
    ///     Gets the cache bust hash for the BackOffice.
    /// </summary>
    string BackOfficeCacheBustHash { get; }

    /// <summary>
    ///     Gets the virtual directory of the BackOffice.
    /// </summary>
    string BackOfficeVirtualDirectory { get; }

    /// <summary>
    ///     Gets the virtual path of the static assets used in the BackOffice.
    /// </summary>
    string BackOfficeAssetsPath { get; }
}
