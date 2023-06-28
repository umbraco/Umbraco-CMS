namespace Umbraco.Cms.Web.Common.Hosting;

/// <summary>
///     Umbraco-specific settings for static files from the Umbraco.Cms.StaticAssets RCL.
/// </summary>
public interface IStaticFilePathGenerator
{
    /// <summary>
    ///     The virtual path of the static assets used in the Backoffice.
    /// </summary>
    string BackofficeAssetsPath { get; }
}
