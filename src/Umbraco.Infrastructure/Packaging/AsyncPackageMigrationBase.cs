using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Infrastructure.Packaging;

public abstract class AsyncPackageMigrationBase : AsyncMigrationBase
{
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IOptions<PackageMigrationSettings> _packageMigrationsSettings;
    private readonly IPackagingService _packagingService;
    private readonly IShortStringHelper _shortStringHelper;

    public AsyncPackageMigrationBase(
        IPackagingService packagingService,
        IMediaService mediaService,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IMigrationContext context,
        IOptions<PackageMigrationSettings> packageMigrationsSettings)
        : base(context)
    {
        _packagingService = packagingService;
        _mediaService = mediaService;
        _mediaFileManager = mediaFileManager;
        _mediaUrlGenerators = mediaUrlGenerators;
        _shortStringHelper = shortStringHelper;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        _packageMigrationsSettings = packageMigrationsSettings;
    }

    public IImportPackageBuilder ImportPackage => BeginBuild(
        new ImportPackageBuilder(
            _packagingService,
            _mediaService,
            _mediaFileManager,
            _mediaUrlGenerators,
            _shortStringHelper,
            _contentTypeBaseServiceProvider,
            Context,
            _packageMigrationsSettings));
}
