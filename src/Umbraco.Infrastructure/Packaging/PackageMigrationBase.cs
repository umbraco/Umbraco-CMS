using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Packaging;

public abstract class PackageMigrationBase : MigrationBase
{
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IOptions<PackageMigrationSettings> _packageMigrationsSettings;
    private readonly IPackagingService _packagingService;
    private readonly IShortStringHelper _shortStringHelper;

    public PackageMigrationBase(
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

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use ctor with all params")]
    public PackageMigrationBase(
        IPackagingService packagingService,
        IMediaService mediaService,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IMigrationContext context)
        : this(
            packagingService,
            mediaService,
            mediaFileManager,
            mediaUrlGenerators,
            shortStringHelper,
            contentTypeBaseServiceProvider,
            context,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<PackageMigrationSettings>>())
    {
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
