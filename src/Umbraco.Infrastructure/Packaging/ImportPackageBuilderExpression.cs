using System.IO.Compression;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging;

internal class ImportPackageBuilderExpression : MigrationExpressionBase
{
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly PackageMigrationSettings _packageMigrationSettings;
    private readonly IPackagingService _packagingService;
    private readonly IShortStringHelper _shortStringHelper;

    private bool _executed;

    public ImportPackageBuilderExpression(
        IPackagingService packagingService,
        IMediaService mediaService,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IMigrationContext context,
        IOptions<PackageMigrationSettings> packageMigrationSettings)
        : base(context)
    {
        _packagingService = packagingService;
        _mediaService = mediaService;
        _mediaFileManager = mediaFileManager;
        _mediaUrlGenerators = mediaUrlGenerators;
        _shortStringHelper = shortStringHelper;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        _packageMigrationSettings = packageMigrationSettings.Value;
    }

    /// <summary>
    ///     The type of the migration which dictates the namespace of the embedded resource
    /// </summary>
    public Type? EmbeddedResourceMigrationType { get; set; }

    public XDocument? PackageDataManifest { get; set; }

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
            throw new InvalidOperationException(
                $"Nothing to execute, neither {nameof(EmbeddedResourceMigrationType)} or {nameof(PackageDataManifest)} has been set.");
        }

        if (!_packageMigrationSettings.RunSchemaAndContentMigrations)
        {
            Logger.LogInformation("Skipping import of embedded schema file, due to configuration");
            return;
        }

        InstallationSummary installationSummary;
        if (EmbeddedResourceMigrationType != null)
        {
            if (PackageMigrationResource.TryGetEmbeddedPackageDataManifest(
                    EmbeddedResourceMigrationType,
                    out XDocument? xml,
                    out ZipArchive? zipPackage))
            {
                // first install the package
                installationSummary = _packagingService.InstallCompiledPackageData(xml!);

                if (zipPackage is not null)
                {
                    // get the embedded resource
                    using (zipPackage)
                    {
                        // then we need to save each file to the saved media items
                        var mediaWithFiles = xml!.XPathSelectElements(
                                "./umbPackage/MediaItems/MediaSet//*[@id][@mediaFilePath]")
                            .ToDictionary(
                                x => x.AttributeValue<Guid>("key"),
                                x => x.AttributeValue<string>("mediaFilePath"));

                        // Any existing media by GUID will not be installed by the package service, it will just be skipped
                        // so you cannot 'update' media (or content) using a package since those are not schema type items.
                        // This means you cannot 'update' the media file either. The installationSummary.MediaInstalled
                        // will be empty for any existing media which means that the files will also not be updated.
                        foreach (IMedia media in installationSummary.MediaInstalled)
                        {
                            if (mediaWithFiles.TryGetValue(media.Key, out var mediaFilePath))
                            {
                                // this is a media item that has a file, so find that file in the zip
                                var entryPath = $"media{mediaFilePath!.EnsureStartsWith('/')}";
                                ZipArchiveEntry? mediaEntry = zipPackage.GetEntry(entryPath);
                                if (mediaEntry == null)
                                {
                                    throw new InvalidOperationException(
                                        "No media file found in package zip for path " +
                                        entryPath);
                                }

                                // read the media file and save it to the media item
                                // using the current file system provider.
                                using (Stream mediaStream = mediaEntry.Open())
                                {
                                    media.SetValue(
                                        _mediaFileManager,
                                        _mediaUrlGenerators,
                                        _shortStringHelper,
                                        _contentTypeBaseServiceProvider,
                                        Constants.Conventions.Media.File,
                                        Path.GetFileName(mediaFilePath)!,
                                        mediaStream);
                                }

                                _mediaService.Save(media);
                            }
                        }
                    }
                }
            }
            else
            {
                installationSummary = _packagingService.InstallCompiledPackageData(PackageDataManifest);
            }

            Logger.LogInformation($"Package migration executed. Summary: {installationSummary}");
        }
    }
}
