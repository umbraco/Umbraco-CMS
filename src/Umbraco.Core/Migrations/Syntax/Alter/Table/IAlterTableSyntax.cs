namespace Umbraco.Core.Migrations.Syntax.Alter.Table
{
    public interface IAlterTableSyntax : IFluentSyntax
    {
        IAlterTableColumnTypeSyntax AddColumn(string name);
        IAlterTableColumnTypeSyntax AlterColumn(string name);
    }
}
