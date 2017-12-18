namespace Umbraco.Core.Migrations.Syntax.Alter.Table
{
    public interface IAlterTableColumnOptionSyntax :
        IColumnOptionSyntax<IAlterTableColumnOptionSyntax, IAlterTableColumnOptionForeignKeyCascadeSyntax>,
        IAlterTableSyntax
    {

    }
}
