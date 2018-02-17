using Umbraco.Core.Persistence.Migrations.Syntax.Check.Column;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Table
{
    public interface ICheckTableSyntax : ICheckExistsSyntax
    {
        ICheckColumnOnTableSyntax WithColumn(string columnName);
        ICheckExistsSyntax WithColumns(string[] columnNames);
    }
}
