using System;
using System.Xml.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Packaging
{
    internal class ImportPackageBuilder : ExpressionBuilderBase<ImportPackageBuilderExpression>, IImportPackageBuilder, IExecutableBuilder
    {
        public ImportPackageBuilder(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context)
            : base(new ImportPackageBuilderExpression(
                packagingService,
                mediaService,
                mediaFileManager,
                mediaUrlGenerators,
                shortStringHelper,
                contentTypeBaseServiceProvider,
                context))
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
