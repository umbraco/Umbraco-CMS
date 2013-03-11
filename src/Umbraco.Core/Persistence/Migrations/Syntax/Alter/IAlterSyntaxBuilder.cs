using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter
{
    public interface IAlterSyntaxBuilder : IFluentSyntax
    {
        IAlterTableSyntax Table(string tableName);
        IAlterColumnSyntax Column(string columnName);
    }
}