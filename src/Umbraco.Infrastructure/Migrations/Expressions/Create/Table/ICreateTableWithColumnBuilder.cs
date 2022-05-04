using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

public interface ICreateTableWithColumnBuilder : IFluentBuilder, IExecutableBuilder
{
    ICreateTableColumnAsTypeBuilder WithColumn(string name);
}
