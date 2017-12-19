using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Create.Column
{
    public interface ICreateColumnOnTableSyntax : IColumnTypeBuilder<ICreateColumnOptionSyntax>
    {
        ICreateColumnTypeSyntax OnTable(string name);
    }
}
