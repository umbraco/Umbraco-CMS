using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;

namespace Umbraco.Cms.Search.Core.Persistence.Migration;

public class CustomPackageMigration : AsyncPackageMigrationBase
{
    public CustomPackageMigration(
        IPackagingService packagingService,
        IMediaService mediaService,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IMigrationContext context,
        IOptions<PackageMigrationSettings> packageMigrationsSettings)
        : base(
            packagingService,
            mediaService,
            mediaFileManager,
            mediaUrlGenerators,
            shortStringHelper,
            contentTypeBaseServiceProvider,
            context,
            packageMigrationsSettings) =>
        RebuildCache = false;

    protected override Task MigrateAsync()
    {
        if (TableExists(Constants.Persistence.IndexDocumentTableName) == false)
        {
            Create.Table<IndexDocumentDto>().Do();
        }
        else
        {
            Logger.LogDebug("The database table {DbTable} already exists, skipping", Constants.Persistence.IndexDocumentTableName);
        }

        return Task.CompletedTask;
    }
}
