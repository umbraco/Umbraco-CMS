// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for package migration settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigPackageMigration)]
public class PackageMigrationSettings
{
    private const bool StaticRunSchemaAndContentMigrations = true;
    private const bool StaticAllowComponentOverrideOfRunSchemaAndContentMigrations = true;

    /// <summary>
    ///     Gets or sets a value indicating whether package migration steps that install schema and content should run.
    /// </summary>
    /// <remarks>
    ///     By default this is true and schema and content defined in a package migration are installed.
    ///     Using configuration, administrators can optionally switch this off in certain environments.
    ///     Deployment tools such as Umbraco Deploy can also configure this option to run or not run these migration
    ///     steps as is appropriate for normal use of the tool.
    /// </remarks>
    [DefaultValue(StaticRunSchemaAndContentMigrations)]
    public bool RunSchemaAndContentMigrations { get; set; } = StaticRunSchemaAndContentMigrations;

    /// <summary>
    ///     Gets or sets a value indicating whether components can override the configured value for
    ///     <see cref="RunSchemaAndContentMigrations" />.
    /// </summary>
    /// <remarks>
    ///     By default this is true and components can override the configured setting for
    ///     <see cref="RunSchemaAndContentMigrations" />.
    ///     If an administrator wants explicit control over which environments migration steps installing schema and content
    ///     can run,
    ///     they can set this to false. Components should respect this and not override the configuration.
    /// </remarks>
    [DefaultValue(StaticAllowComponentOverrideOfRunSchemaAndContentMigrations)]
    public bool AllowComponentOverrideOfRunSchemaAndContentMigrations { get; set; } =
        StaticAllowComponentOverrideOfRunSchemaAndContentMigrations;
}
