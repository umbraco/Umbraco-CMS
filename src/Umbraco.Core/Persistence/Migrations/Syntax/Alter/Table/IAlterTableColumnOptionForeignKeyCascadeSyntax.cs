namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table
{
    public interface IAlterTableColumnOptionForeignKeyCascadeSyntax :
        IAlterTableColumnOptionSyntax,
        IForeignKeyCascadeSyntax<IAlterTableColumnOptionSyntax, IAlterTableColumnOptionForeignKeyCascadeSyntax>
    {
         
    }
}