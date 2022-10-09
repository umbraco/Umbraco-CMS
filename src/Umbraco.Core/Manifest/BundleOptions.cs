namespace Umbraco.Cms.Core.Manifest;

public enum BundleOptions
{
    /// <summary>
    ///     The default bundling behavior for assets in the package folder.
    /// </summary>
    /// <remarks>
    ///     The assets will be bundled with the typical packages bundle.
    /// </remarks>
    Default = 0,

    /// <summary>
    ///     The assets in the package will not be processed at all and will all be requested as individual assets.
    /// </summary>
    /// <remarks>
    ///     This will essentially be a bundle that has composite processing turned off for both debug and production.
    /// </remarks>
    None = 1,

    /// <summary>
    ///     The packages assets will be processed as it's own separate bundle. (in debug, files will not be processed)
    /// </summary>
    Independent = 2,
}
