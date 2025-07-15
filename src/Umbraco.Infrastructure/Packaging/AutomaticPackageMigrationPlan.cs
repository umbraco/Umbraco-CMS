using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging;

/// <summary>
/// Represents a package migration plan that automatically imports an embedded package data manifest.
/// </summary>
public abstract class AutomaticPackageMigrationPlan : PackageMigrationPlan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutomaticPackageMigrationPlan" /> class.
    /// </summary>
    /// <param name="packageName">The package name that the plan is for. If the package has a package.manifest these must match.</param>
    protected AutomaticPackageMigrationPlan(string packageName)
        : this(packageName, packageName)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutomaticPackageMigrationPlan" /> class.
    /// </summary>
    /// <param name="packageName">The package name that the plan is for. If the package has a package.manifest these must match.</param>
    /// <param name="planName">The plan name for the package. This should be the same name as the package name, if there is only one plan in the package.</param>
    protected AutomaticPackageMigrationPlan(string packageName, string planName)
        : this(null!, packageName, planName)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutomaticPackageMigrationPlan" /> class.
    /// </summary>
    /// <param name="packageId">The package identifier that the plan is for. If the package has a package.manifest these must match.</param>
    /// <param name="packageName">The package name that the plan is for. If the package has a package.manifest these must match.</param>
    /// <param name="planName">The plan name for the package. This should be the same name as the package name, if there is only one plan in the package.</param>
    protected AutomaticPackageMigrationPlan(string packageId, string packageName, string planName)
        : base(packageId, packageName, planName)
    { }

    /// <inheritdoc />
    protected sealed override void DefinePlan()
    {
        // calculate the final state based on the hash value of the embedded resource
        Type planType = GetType();
        var hash = PackageMigrationResource.GetEmbeddedPackageDataManifestHash(planType);

        var finalId = hash.ToGuid();
        To<MigrateToPackageData>(finalId);
    }

    /// <summary>
    /// Provides a migration that imports an embedded package data manifest.
    /// </summary>
    private sealed class MigrateToPackageData : AsyncPackageMigrationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateToPackageData" /> class.
        /// </summary>
        /// <param name="packagingService">The packaging service.</param>
        /// <param name="mediaService">The media service.</param>
        /// <param name="mediaFileManager">The media file manager.</param>
        /// <param name="mediaUrlGenerators">The media URL generators.</param>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="contentTypeBaseServiceProvider">The content type base service provider.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="options">The package migration settings.</param>
        public MigrateToPackageData(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context,
            IOptions<PackageMigrationSettings> options)
            : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context, options)
        { }

        /// <inheritdoc />
        protected override Task MigrateAsync()
        {
            var plan = (AutomaticPackageMigrationPlan)Context.Plan;

            ImportPackage.FromEmbeddedResource(plan.GetType()).Do();

            return Task.CompletedTask;
        }
    }
}
