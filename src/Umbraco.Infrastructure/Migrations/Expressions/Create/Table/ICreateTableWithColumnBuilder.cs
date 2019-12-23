using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Create.Table
{
    public interface ICreateTableWithColumnBuilder : IFluentBuilder, IExecutableBuilder
    {
        ICreateTableColumnAsTypeBuilder WithColumn(string name);
    }
}
