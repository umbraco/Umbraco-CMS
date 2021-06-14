using System;
using System.Xml.Linq;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging
{
    /// <summary>
    /// Used to automatically indicate that a package has an embedded package data manifest that needs to be installed
    /// </summary>
    public abstract class AutomaticPackageMigrationPlan : PackageMigrationPlan
    {
        protected AutomaticPackageMigrationPlan(string name)
            : base(name)
        {
        }

        protected sealed override void DefinePlan()
        {
            // calculate the final state based on the hash value of the embedded resource
            Type planType = GetType();
            PackageDataManifest = PackageMigrationResource.GetEmbeddedPackageDataManifest(planType);
            var finalId = PackageDataManifest.ToString(SaveOptions.DisableFormatting).ToGuid();

            To<MigrateToPackageData>(finalId);
        }

        /// <summary>
        /// Get/set the extracted package data xml manifest
        /// </summary>
        private XDocument PackageDataManifest { get; set; }

        private class MigrateToPackageData : PackageMigrationBase
        {
            public MigrateToPackageData(IPackagingService packagingService, IMigrationContext context)
                : base(packagingService, context)
            {
            }

            public override void Migrate()
            {
                var plan = (AutomaticPackageMigrationPlan)Context.Plan;
                XDocument xml = plan.PackageDataManifest;
                ImportPackage.FromXmlDataManifest(xml).Do();
            }
        }
    }
}
