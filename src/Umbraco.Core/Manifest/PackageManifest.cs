namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Represents a package manifest that defines metadata and extensions for an Umbraco package.
/// </summary>
public class PackageManifest
{
    /// <summary>
    ///     Gets or sets the name of the package.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the package.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     Gets or sets the version of the package.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the package allows public access.
    /// </summary>
    public bool AllowPublicAccess { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether Umbraco <em>automatically</em> appends a cache-buster
    ///     (<c>?v=&lt;version&gt;&amp;umb__rnd=&lt;host cache-buster&gt;</c>) to this package's clean
    ///     <c>/App_Plugins</c> JavaScript URLs, in both its importmap (server-side) and its registered extensions
    ///     (client-side). <c>true</c> by default; set to <c>false</c> to opt out of the automatic stamping. URLs that
    ///     already carry a query string are always left untouched. This setting controls only the automatic stamping —
    ///     an explicit <c>%CACHE_BUSTER%</c> token authored into a URL always resolves regardless of this value.
    /// </summary>
    public bool AllowCacheBusting { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the package allows telemetry collection.
    /// </summary>
    public bool AllowTelemetry { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the package allows telemetry collection.
    /// </summary>
    [Obsolete("Use AllowTelemetry instead. Scheduled for removal in Umbraco 18.")]
    public bool AllowPackageTelemetry { get; set; } = true;

    /// <summary>
    ///     Gets or sets the array of extension objects defined by this package.
    /// </summary>
    public required object[] Extensions { get; set; }

    /// <summary>
    ///     Gets or sets the importmap configuration for the package.
    /// </summary>
    public PackageManifestImportmap? Importmap { get; set; }
}
