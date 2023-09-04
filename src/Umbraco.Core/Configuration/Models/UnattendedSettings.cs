// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for unattended settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigUnattended)]
public class UnattendedSettings
{
    private const bool StaticInstallUnattended = false;
    private const bool StaticUpgradeUnattended = false;

    /// <summary>
    ///     Gets or sets a value indicating whether unattended installs are enabled.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, when a database connection string is configured and it is possible to connect to
    ///         the database, but the database is empty, the runtime enters the <c>Install</c> level.
    ///         If this option is set to <c>true</c> an unattended install will be performed and the runtime enters
    ///         the <c>Run</c> level.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticInstallUnattended)]
    public bool InstallUnattended { get; set; } = StaticInstallUnattended;

    /// <summary>
    ///     Gets or sets a value indicating whether unattended upgrades are enabled.
    /// </summary>
    [DefaultValue(StaticUpgradeUnattended)]
    public bool UpgradeUnattended { get; set; } = StaticUpgradeUnattended;

    /// <summary>
    ///     Gets or sets a value indicating whether unattended package migrations are enabled.
    /// </summary>
    /// <remarks>
    ///     This is true by default.
    /// </remarks>
    public bool PackageMigrationsUnattended { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value to use for creating a user with a name for Unattended Installs
    /// </summary>
    public string? UnattendedUserName { get; set; } = null;

    /// <summary>
    ///     Gets or sets a value to use for creating a user with an email for Unattended Installs
    /// </summary>
    [EmailAddress]
    public string? UnattendedUserEmail { get; set; } = null;

    /// <summary>
    ///     Gets or sets a value to use for creating a user with a password for Unattended Installs
    /// </summary>
    public string? UnattendedUserPassword { get; set; } = null;
}
