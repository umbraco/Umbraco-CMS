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

        protected override string GetInitialState()
        {
            // no state in database yet - assume we have something in web.config that makes some sense
            if (!SemVersion.TryParse(ConfigurationManager.AppSettings["umbracoConfigurationStatus"], out var currentVersion))
                throw new InvalidOperationException("Could not get current version from web.config umbracoConfigurationStatus appSetting.");

            // must be at least 7.8.0 - fixme adjust when releasing
            if (currentVersion < new SemVersion(7, 8)) 
                throw new InvalidOperationException($"Version {currentVersion} cannot be upgraded to {UmbracoVersion.SemanticVersion}.");

            // cannot go back in time
            if (currentVersion > UmbracoVersion.SemanticVersion)
                throw new InvalidOperationException($"Version {currentVersion} cannot be upgraded to {UmbracoVersion.SemanticVersion}.");

            switch (currentVersion.Major)
            {
                case 7:
                    return "{orig-" + currentVersion + "}";
                case 8: // fixme remove when releasing
                    // this is very temp and for my own website - zpqrtbnk
                    return "{04F54303-3055-4700-8F76-35A37F232FF5}"; // right before the variants migration
                default:
                    throw new InvalidOperationException($"Version {currentVersion} should have an upgrade state in the key-value table.");
            }
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
