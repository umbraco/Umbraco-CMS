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
    ///     Gets or sets a value indicating whether automatic cache-busting of this package's
    ///     <c>/App_Plugins</c> importmap assets is disabled. When <c>false</c> (default), Umbraco appends a
    ///     per-package <c>?umb__rnd</c> token derived from <see cref="Version"/> to the package's importmap URLs.
    /// </summary>
    public bool DisableCacheBusting { get; set; }

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
