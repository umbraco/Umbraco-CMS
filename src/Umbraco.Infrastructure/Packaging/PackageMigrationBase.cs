using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute;

namespace Umbraco.Cms.Infrastructure.Packaging
{

    public abstract class PackageMigrationBase : MigrationBase
    {
        private readonly IPackagingService _packagingService;
        private readonly MediaFileManager _mediaFileManager;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IJsonSerializer _jsonSerializer;

        public PackageMigrationBase(
            IPackagingService packagingService,
            MediaFileManager mediaFileManager,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IJsonSerializer jsonSerializer,
            IMigrationContext context)
            : base(context)
        {
            _packagingService = packagingService;
            _mediaFileManager = mediaFileManager;
            _shortStringHelper = shortStringHelper;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _jsonSerializer = jsonSerializer;
        }

        public IImportPackageBuilder ImportPackage => BeginBuild(
            new ImportPackageBuilder(
                _packagingService,
                _mediaFileManager,
                _shortStringHelper,
                _contentTypeBaseServiceProvider,
                _jsonSerializer,
                Context));

    }
}
