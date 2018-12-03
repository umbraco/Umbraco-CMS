using System;
using System.Configuration;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Upgrade
{
    public class UmbracoUpgrader : Upgrader
    {
        public UmbracoUpgrader(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, PostMigrationCollection postMigrations, ILogger logger)
            : base(scopeProvider, migrationBuilder, keyValueService, postMigrations, logger)
        { }

        protected override MigrationPlan GetPlan()
        {
            return new UmbracoPlan(MigrationBuilder, Logger);
        }

        protected override (SemVersion, SemVersion) GetVersions()
        {
            // assume we have something in web.config that makes some sense
            if (!SemVersion.TryParse(ConfigurationManager.AppSettings["umbracoConfigurationStatus"], out var currentVersion))
                throw new InvalidOperationException("Could not get current version from web.config umbracoConfigurationStatus appSetting.");

            return (currentVersion, UmbracoVersion.SemanticVersion);
        }
    }
}
