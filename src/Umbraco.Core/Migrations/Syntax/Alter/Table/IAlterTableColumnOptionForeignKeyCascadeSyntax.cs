namespace Umbraco.Core.Migrations.Syntax.Alter.Table
{
    public interface IAlterTableColumnOptionForeignKeyCascadeSyntax :
        IAlterTableColumnOptionSyntax,
        IForeignKeyCascadeSyntax<IAlterTableColumnOptionSyntax, IAlterTableColumnOptionForeignKeyCascadeSyntax>
    {

    }
}
