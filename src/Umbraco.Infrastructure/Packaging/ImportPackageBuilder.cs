using System;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Packaging
{
    internal class ImportPackageBuilder : ExpressionBuilderBase<ImportPackageBuilderExpression>, IImportPackageBuilder, IExecutableBuilder
    {
        public ImportPackageBuilder(IPackagingService packagingService, IMigrationContext context)
            : base(new ImportPackageBuilderExpression(packagingService, context))
        {
        }

        public void Do() => Expression.Execute();

        public IExecutableBuilder FromEmbeddedResource<TPackageMigration>()
            where TPackageMigration : PackageMigrationBase
        {
            Expression.EmbeddedResourceMigrationType = typeof(TPackageMigration);
            return this;
        }

        public IExecutableBuilder FromEmbeddedResource(Type packageMigrationType)
        {
            Expression.EmbeddedResourceMigrationType = packageMigrationType;
            return this;
        }

        public IExecutableBuilder FromXmlDataManifest(XDocument packageDataManifest)
        {
            Expression.PackageDataManifest = packageDataManifest;
            return this;
        }
    }
}
