using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging;

/// <summary>
///     Used to automatically indicate that a package has an embedded package data manifest that needs to be installed
/// </summary>
public abstract class AutomaticPackageMigrationPlan : PackageMigrationPlan
{
    protected AutomaticPackageMigrationPlan(string packageName)
        : this(packageName, packageName)
    {
    }

    protected AutomaticPackageMigrationPlan(string packageName, string planName)
        : base(packageName, planName)
    {
    }

    protected sealed override void DefinePlan()
    {
        // calculate the final state based on the hash value of the embedded resource
        Type planType = GetType();
        var hash = PackageMigrationResource.GetEmbeddedPackageDataManifestHash(planType);

        var finalId = hash.ToGuid();
        To<MigrateToPackageData>(finalId);
    }

    private class MigrateToPackageData : PackageMigrationBase
    {
        public MigrateToPackageData(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context)
            : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context)
        {
        }

        protected override void Migrate()
        {
            var plan = (AutomaticPackageMigrationPlan)Context.Plan;

            ImportPackage.FromEmbeddedResource(plan.GetType()).Do();
        }
    }
}
