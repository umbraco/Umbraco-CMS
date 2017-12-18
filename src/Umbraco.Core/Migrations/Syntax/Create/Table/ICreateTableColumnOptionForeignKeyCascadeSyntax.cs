namespace Umbraco.Core.Migrations.Syntax.Create.Table
{
    public interface ICreateTableColumnOptionForeignKeyCascadeSyntax :
        ICreateTableColumnOptionSyntax,
        IForeignKeyCascadeSyntax<ICreateTableColumnOptionSyntax, ICreateTableColumnOptionForeignKeyCascadeSyntax>
    {

    }
}
