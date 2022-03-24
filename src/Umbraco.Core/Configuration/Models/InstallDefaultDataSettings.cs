// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;

namespace Umbraco.Cms.Core.Configuration.Models
{
    public enum InstallDefaultDataOption
    {
        None,
        InstallOnly,
        InstallAllExcept,
        All
    }

    /// <summary>
    /// Typed configuration options for installation of default data.
    /// </summary>
    public class InstallDefaultDataSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to create default data on installation.
        /// </summary>
        public InstallDefaultDataOption InstallData { get; set; } = InstallDefaultDataOption.All;

        /// <summary>
        /// Gets or sets a value indicating which default data (languages, data types, etc.) should be created when <see cref="InstallData"/> is
        /// set to <see cref="InstallDefaultDataOption.InstallOnly"/> or <see cref="InstallDefaultDataOption.InstallAllExcept"/>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         For languages, the values provided should be the ISO codes for the languages to be included or excluded, e.g. "en-US".
        ///         If removing the single default language, ensure that a different one is created via some other means (such
        ///         as a restore from Umbraco Deploy schema data).
        ///     </para>
        ///     <para>
        ///         For data types, the values provided should be the Guid values used by Umbraco for the data type, listed at:
        ///         <see cref="Constants.DataTypes"/>
        ///         Some data types - such as the string label - cannot be excluded from install as they are required for core Umbraco
        ///         functionality.
        ///         Otherwise take care not to remove data types required for default Umbraco media and member types, unless you also
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
        public List<string> SelectedValues { get; set; } = new List<string>();
    }
}
