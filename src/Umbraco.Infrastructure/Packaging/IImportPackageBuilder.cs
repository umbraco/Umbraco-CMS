using Umbraco.Cms.Infrastructure.Migrations.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Core.Packaging
{
    public interface IImportPackageBuilder : IFluentBuilder
    {
        IExecutableBuilder FromEmbeddedResource();
    }
}
