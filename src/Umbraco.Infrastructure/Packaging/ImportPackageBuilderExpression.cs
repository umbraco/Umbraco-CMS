using System;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Packaging
{
    internal class ImportPackageBuilderExpression : MigrationExpressionBase
    {
        private readonly IPackagingService _packagingService;
        private bool _executed;

        public ImportPackageBuilderExpression(IPackagingService packagingService, IMigrationContext context) : base(context)
            => _packagingService = packagingService;

        /// <summary>
        /// The type of the migration which dictates the namespace of the embedded resource
        /// </summary>
        public Type EmbeddedResourceMigrationType { get; set; }

        public XDocument PackageDataManifest { get; set; }

        public override void Execute()
        {
            if (_executed)
            {
                throw new InvalidOperationException("This expression has already been executed.");
            }

            _executed = true;
            Context.BuildingExpression = false;

            if (EmbeddedResourceMigrationType == null && PackageDataManifest == null)
            {
                throw new InvalidOperationException($"Nothing to execute, neither {nameof(EmbeddedResourceMigrationType)} or {nameof(PackageDataManifest)} has been set.");
            }

            XDocument xml;
            if (EmbeddedResourceMigrationType != null)
            {
                // get the embedded resource
                xml = PackageMigrationResource.GetEmbeddedPackageDataManifest(EmbeddedResourceMigrationType);
            }
            else
            {
                xml = PackageDataManifest;
            }
            
            InstallationSummary installationSummary = _packagingService.InstallCompiledPackageData(xml);

            Logger.LogInformation($"Package migration executed. Summary: {installationSummary}");
        }
    }
}
