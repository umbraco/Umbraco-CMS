// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     An enumeration of options available for control over installation of default Umbraco data.
/// </summary>
public enum InstallDefaultDataOption
{
    /// <summary>
    ///     Do not install any items of this type (other than Umbraco defined essential ones).
    /// </summary>
    None,

    /// <summary>
    ///     Only install the default data specified in the <see cref="InstallDefaultDataSettings.Values" />
    /// </summary>
    Values,

    /// <summary>
    ///     Install all default data, except that specified in the <see cref="InstallDefaultDataSettings.Values" />
    /// </summary>
    ExceptValues,

    /// <summary>
    ///     Install all default data.
    /// </summary>
    All,
}

/// <summary>
///     Typed configuration options for installation of default data.
/// </summary>
public class InstallDefaultDataSettings
{
    /// <summary>
    ///     Gets or sets a value indicating whether to create default data on installation.
    /// </summary>
    public InstallDefaultDataOption InstallData { get; set; } = InstallDefaultDataOption.All;

    /// <summary>
    ///     Gets or sets a value indicating which default data (languages, data types, etc.) should be created when
    ///     <see cref="InstallData" /> is
    ///     set to <see cref="InstallDefaultDataOption.Values" /> or <see cref="InstallDefaultDataOption.ExceptValues" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For languages, the values provided should be the ISO codes for the languages to be included or excluded, e.g.
    ///         "en-US".
    ///         If removing the single default language, ensure that a different one is created via some other means (such
    ///         as a restore from Umbraco Deploy schema data).
    ///     </para>
    ///     <para>
    ///         For data types, the values provided should be the Guid values used by Umbraco for the data type, listed at:
    ///         <see cref="Constants.DataTypes" />
    ///         Some data types - such as the string label - cannot be excluded from install as they are required for core
    ///         Umbraco
    ///         functionality.
    ///         Otherwise take care not to remove data types required for default Umbraco media and member types, unless you
    ///         also
    ///         choose to exclude them.
    ///     </para>
    ///     <para>
    ///         For media types, the values provided should be the Guid values used by Umbraco for the media type, listed at:
    ///         https://github.com/umbraco/Umbraco-CMS/blob/v9/dev/src/Umbraco.Infrastructure/Migrations/Install/DatabaseDataCreator.cs.
    ///     </para>
    ///     <para>
    ///         For member types, the values provided should be the Guid values used by Umbraco for the member type, listed at:
    ///         https://github.com/umbraco/Umbraco-CMS/blob/v9/dev/src/Umbraco.Infrastructure/Migrations/Install/DatabaseDataCreator.cs.
    ///     </para>
    /// </remarks>
    public IList<string> Values { get; set; } = new List<string>();
}
