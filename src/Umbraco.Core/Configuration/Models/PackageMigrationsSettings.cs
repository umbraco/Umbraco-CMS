// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for global settings.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigPackageMigrations)]
    public class PackageMigrationsSettings
    {

        private const bool StaticDisableImportFromEmbeddedSchema = false;

        /// <summary>
        /// An option to disable the import from embedded schema files using package migrations.
        /// </summary>
        [DefaultValue(StaticDisableImportFromEmbeddedSchema)]
        public bool DisableImportFromEmbeddedSchema { get; set; } = StaticDisableImportFromEmbeddedSchema;

    }
}
