using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging
{
    internal class ImportPackageBuilderExpression : MigrationExpressionBase
    {
        private readonly IPackagingService _packagingService;
        private readonly MediaFileManager _mediaFileManager;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private bool _executed;

        public ImportPackageBuilderExpression(
            IPackagingService packagingService,
            MediaFileManager mediaFileManager,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IJsonSerializer jsonSerializer,
            IMigrationContext context) : base(context)
        {
            _packagingService = packagingService;
            _mediaFileManager = mediaFileManager;
            _shortStringHelper = shortStringHelper;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _jsonSerializer = jsonSerializer;
        }

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

            InstallationSummary installationSummary;
            if (EmbeddedResourceMigrationType != null)
            {
                // get the embedded resource
                using (ZipArchive zipPackage = PackageMigrationResource.GetEmbeddedPackageDataManifest(
                    EmbeddedResourceMigrationType,
                    out XDocument xml))
                {
                    // first install the package
                    installationSummary = _packagingService.InstallCompiledPackageData(xml);

                    // then we need to save each file to the saved media items
                    var mediaWithFiles = xml.XPathSelectElements(
                        "./umbPackage/MediaItems/MediaSet//*[@id][@mediaFilePath]")
                        .ToDictionary(
                            x => x.AttributeValue<int>("id"),
                            x => x.AttributeValue<string>("mediaFilePath"));

                    // TODO: Almost works! Just wonder if the media installed list is empty because nothing changed?

                    foreach(IMedia media in installationSummary.MediaInstalled)
                    {
                        if (mediaWithFiles.TryGetValue(media.Id, out var mediaFilePath))
                        {
                            // this is a media item that has a file, so find that file in the zip
                            string entryName = mediaFilePath.TrimStart('/');
                            ZipArchiveEntry mediaEntry = zipPackage.GetEntry(entryName);
                            if (mediaEntry == null)
                            {
                                throw new InvalidOperationException("No media file found in package zip for path " + entryName);
                            }

                            // read the media file and save it to the media item
                            // using the current file system provider.
                            using (Stream mediaStream = mediaEntry.Open())
                            {
                                media.SetValue(
                                    _mediaFileManager,
                                    _shortStringHelper,
                                    _contentTypeBaseServiceProvider,
                                    _jsonSerializer,
                                    Constants.Conventions.Media.File,
                                    Path.GetFileName(mediaFilePath),
                                    mediaStream);
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
