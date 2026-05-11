using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Infrastructure.Packaging;

/// <inheritdoc />
[Obsolete("Use AsyncPackageMigrationBase instead. Scheduled for removal in Umbraco 18.")]
public abstract class PackageMigrationBase : AsyncPackageMigrationBase
{
    protected PackageMigrationBase(
        IPackagingService packagingService,
        IMediaService mediaService,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IMigrationContext context,
        IOptions<PackageMigrationSettings> packageMigrationsSettings)
        : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context, packageMigrationsSettings)
    { }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        Migrate();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the migration.
    /// </summary>
    protected abstract void Migrate();
}
