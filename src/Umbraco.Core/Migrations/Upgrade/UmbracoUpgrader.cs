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
        private readonly PostMigrationCollection _postMigrations;

        public UmbracoUpgrader(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, PostMigrationCollection postMigrations, ILogger logger)
            : base(scopeProvider, migrationBuilder, keyValueService, logger)
        {
            _postMigrations = postMigrations ?? throw new ArgumentNullException(nameof(postMigrations));
        }

        protected override MigrationPlan CreatePlan()
        {
            return new UmbracoPlan(MigrationBuilder, Logger);
        }

        protected (SemVersion, SemVersion) GetVersions()
        {
            // assume we have something in web.config that makes some sense
            if (!SemVersion.TryParse(ConfigurationManager.AppSettings["umbracoConfigurationStatus"], out var currentVersion))
                throw new InvalidOperationException("Could not get current version from web.config umbracoConfigurationStatus appSetting.");

            return (currentVersion, UmbracoVersion.SemanticVersion);
        }

        public override void AfterMigrations(IScope scope)
        {
            // run post-migrations
            var (originVersion, targetVersion) = GetVersions();

            foreach (var postMigration in _postMigrations)
                postMigration.Execute(Name, scope, originVersion, targetVersion, Logger);
        }
    }
}
