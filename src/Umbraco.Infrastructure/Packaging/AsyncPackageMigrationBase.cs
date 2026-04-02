using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Infrastructure.Packaging;

/// <summary>
/// Serves as a base class for implementing asynchronous migrations related to package installation or upgrades in Umbraco.
/// </summary>
public abstract class AsyncPackageMigrationBase : AsyncMigrationBase
{
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IOptions<PackageMigrationSettings> _packageMigrationsSettings;
    private readonly IPackagingService _packagingService;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Packaging.AsyncPackageMigrationBase"/> class.
    /// </summary>
    /// <param name="packagingService">The <see cref="IPackagingService"/> used for package operations.</param>
    /// <param name="mediaService">The <see cref="IMediaService"/> used for media management.</param>
    /// <param name="mediaFileManager">The <see cref="MediaFileManager"/> responsible for handling media files.</param>
    /// <param name="mediaUrlGenerators">A collection of <see cref="MediaUrlGeneratorCollection"/> used to generate media URLs.</param>
    /// <param name="shortStringHelper">The <see cref="IShortStringHelper"/> used for string manipulation and formatting.</param>
    /// <param name="contentTypeBaseServiceProvider">The <see cref="IContentTypeBaseServiceProvider"/> for accessing content type services.</param>
    /// <param name="context">The <see cref="IMigrationContext"/> providing context for the migration process.</param>
    /// <param name="packageMigrationsSettings">The <see cref="IOptions{PackageMigrationSettings}"/> containing settings for package migrations.</param>
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

    /// <summary>
    /// Gets an <see cref="IImportPackageBuilder"/> instance to initiate the package import process.
    /// Use this builder to configure and execute the import of a package into the system.
    /// </summary>
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
