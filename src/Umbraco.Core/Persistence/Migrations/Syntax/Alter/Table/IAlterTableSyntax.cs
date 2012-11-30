namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table
{
    public interface IAlterTableSyntax : IFluentSyntax
    {
        IAlterTableColumnSyntax AddColumn(string name);
        IAlterTableColumnSyntax AlterColumn(string name);
    }
}