namespace Umbraco.Cms.Core.WebAssets;

/// <summary>
/// Used for bundling and minifying web assets at runtime
/// </summary>
public interface IRuntimeMinifier
{
    /// <summary>
    /// Returns the cache buster value
    /// </summary>
    string CacheBuster { get; }

    /// <summary>
    /// Creates a css bundle
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="bundleOptions"></param>
    /// <param name="filePaths"></param>
    /// <remarks>
    /// All files must be absolute paths, relative paths will throw <see cref="InvalidOperationException" />
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if any of the paths specified are not absolute
    /// </exception>
    void CreateCssBundle(string bundleName, BundlingOptions bundleOptions, params string[]? filePaths);

    /// <summary>
    ///  Renders the html link tag for the bundle
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns>
    /// An html encoded string
    /// </returns>
    Task<string> RenderCssHereAsync(string bundleName);

    /// <summary>
    ///     Creates a JS bundle
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="bundleOptions"></param>
    /// <param name="filePaths"></param>
    /// <remarks>
    /// All files must be absolute paths, relative paths will throw <see cref="InvalidOperationException" />
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if any of the paths specified are not absolute
    /// </exception>
    void CreateJsBundle(string bundleName, BundlingOptions bundleOptions, params string[]? filePaths);

    /// <summary>
    /// Renders the html script tag for the bundle
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns>
    /// An html encoded string
    /// </returns>
    Task<string> RenderJsHereAsync(string bundleName);

    /// <summary>
    /// Returns the asset paths for the JS bundle name
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns>
    /// If debug mode is enabled this will return all asset paths (not bundled), else it will return a bundle URL
    /// </returns>
    Task<IEnumerable<string>> GetJsAssetPathsAsync(string bundleName);

    /// <summary>
    /// Returns the asset paths for the css bundle name
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns>
    /// If debug mode is enabled this will return all asset paths (not bundled), else it will return a bundle URL
    /// </returns>
    Task<IEnumerable<string>> GetCssAssetPathsAsync(string bundleName);

    /// <summary>
    /// Minify the file content, of a given type
    /// </summary>
    /// <param name="fileContent"></param>
    /// <param name="assetType"></param>
    /// <returns></returns>
    Task<string> MinifyAsync(string? fileContent, AssetType assetType);
}
