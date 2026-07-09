// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for hosting settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigHosting)]
public class HostingSettings
{
    /// <summary>
    ///     The default local temporary storage location.
    /// </summary>
    internal const string StaticLocalTempStorageLocation = "Default";

    /// <summary>
    ///     The default value for debug mode.
    /// </summary>
    internal const bool StaticDebug = false;

    /// <summary>
    ///     Gets or sets a value for the application virtual path.
    /// </summary>
    public string? ApplicationVirtualPath { get; set; }

    /// <summary>
    ///     Gets or sets a value for the location of temporary files.
    /// </summary>
    [DefaultValue(StaticLocalTempStorageLocation)]
    public LocalTempStorage LocalTempStorageLocation { get; set; } = Enum.Parse<LocalTempStorage>(StaticLocalTempStorageLocation);

    /// <summary>
    /// Gets or sets a value for the location of temporary file uploads.
    /// </summary>
    /// <value>/umbraco/Data/TEMP/TemporaryFile if nothing is specified.</value>
    public string? TemporaryFileUploadLocation { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether umbraco is running in [debug mode].
    /// </summary>
    /// <value><c>true</c> if [debug mode]; otherwise, <c>false</c>.</value>
    [DefaultValue(StaticDebug)]
    public bool Debug { get; set; } = StaticDebug;

    /// <summary>
    ///     Gets or sets a value specifying the name of the site.
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    ///     Gets or sets a stable identifier for this server instance used to track cache synchronization state.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Set this when the machine name is not stable across restarts — for example on Azure App Service Linux,
    ///         where the container hostname changes on each recycle. Use a value that is unique per server instance
    ///         (e.g. the site name for single-server deployments, or a per-instance value for scale-out).
    ///     </para>
    ///     <para>
    ///         When not set, Umbraco automatically uses the <c>WEBSITE_INSTANCE_ID</c> environment variable on
    ///         Azure App Service, or falls back to <c>Environment.MachineName</c>.
    ///     </para>
    ///     <para>
    ///         The value of <see cref="SiteName" /> is still appended when set, matching the behaviour of the
    ///         default <c>Environment.MachineName</c>-based identifier.
    ///     </para>
    ///     <para>The combined value of this setting and <see cref="SiteName" /> must not exceed 255 characters.</para>
    /// </remarks>
    public string? MachineIdentifier { get; set; }
}
