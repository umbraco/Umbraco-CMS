namespace Umbraco.Cms.Web.Common.Hosting;

/// <summary>
///     Umbraco-specific settings for static files from the Umbraco.Cms.StaticAssets RCL.
/// </summary>
public interface IStaticFilePathGenerator
{
    /// <summary>
    ///     The virtual path of the Backoffice.
    /// </summary>
    string BackofficePath { get; }

    /// <summary>
    ///     The virtual path of the static assets used in the Backoffice.
    /// </summary>
    string BackofficeAssetsPath { get; }


    /// <summary>
    ///     Returns the exports for the BackOffice package.
    /// </summary>
    /// <remarks>It will read the umbraco-package.json files to calculate the list of files.</remarks>
    /// <returns>A list of file paths that is required for the BackOffice to run.</returns>
    Task<string> GetBackofficePackageExportsAsync();
}
