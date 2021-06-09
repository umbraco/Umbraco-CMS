using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Core.Packaging
{
    internal class ImportPackageBuilder : ExpressionBuilderBase<ImportPackageBuilderExpression>, IImportPackageBuilder, IExecutableBuilder
    {
        public ImportPackageBuilder(IPackagingService packagingService, IMigrationContext context)
            : base(new ImportPackageBuilderExpression(packagingService, context))
        {
        }

        public void Do() => Expression.Execute();

        public IExecutableBuilder FromEmbeddedResource()
        {
            Expression.FromEmbeddedResource = true;
            return this;
        }
    }
}
