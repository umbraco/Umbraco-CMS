namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table
{
    public interface IAlterTableColumnOptionSyntax : 
        IColumnOptionSyntax<IAlterTableColumnOptionSyntax, IAlterTableColumnOptionForeignKeyCascadeSyntax>,
        IAlterTableSyntax
    {
         
    }
}