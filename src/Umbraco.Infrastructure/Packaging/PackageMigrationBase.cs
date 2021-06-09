using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute;

namespace Umbraco.Cms.Core.Packaging
{

    public abstract class PackageMigrationBase : MigrationBase
    {
        private readonly IPackagingService _packagingService;

        public PackageMigrationBase(IPackagingService packagingService, IMigrationContext context) : base(context)
            => _packagingService = packagingService;

        public IImportPackageBuilder ImportPackage => BeginBuild(new ImportPackageBuilder(_packagingService, Context));

    }
}
