using System;
using System.Configuration;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Upgrade
{
    /// <summary>
    /// Represents the Umbraco upgrader.
    /// </summary>
    public class UmbracoUpgrader : Upgrader
    {
        private PostMigrationCollection _postMigrations;

        /// <summary>
        /// Initializes a new instance of the <see ref="UmbracoUpgrader" /> class.
        /// </summary>
        public UmbracoUpgrader()
            : base(new UmbracoPlan())
        { }

        /// <summary>
        /// Executes.
        /// </summary>
        public void Execute(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger, PostMigrationCollection postMigrations)
        {
            _postMigrations = postMigrations;
            Execute(scopeProvider, migrationBuilder, keyValueService, logger);
        }

        /// <inheritdoc />
        public override void AfterMigrations(IScope scope, ILogger logger)
        {
            // assume we have something in web.config that makes some sense = the origin version
            if (!SemVersion.TryParse(ConfigurationManager.AppSettings["umbracoConfigurationStatus"], out var originVersion))
                throw new InvalidOperationException("Could not get current version from web.config umbracoConfigurationStatus appSetting.");

            // target version is the code version
            var targetVersion = UmbracoVersion.SemanticVersion;

            foreach (var postMigration in _postMigrations)
                postMigration.Execute(Name, scope, originVersion, targetVersion, logger);
        }
    }
}
