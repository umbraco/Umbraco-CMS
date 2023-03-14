using System.Xml.Linq;
using Umbraco.Cms.Infrastructure.Migrations.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Packaging;

public interface IImportPackageBuilder : IFluentBuilder
{
    IExecutableBuilder FromEmbeddedResource<TPackageMigration>()
        where TPackageMigration : PackageMigrationBase;

    IExecutableBuilder FromEmbeddedResource(Type packageMigrationType);

    IExecutableBuilder FromXmlDataManifest(XDocument packageDataManifest);
}
