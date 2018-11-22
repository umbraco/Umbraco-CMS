namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Table
{
    public interface ICreateTableColumnOptionForeignKeyCascadeSyntax :
        ICreateTableColumnOptionSyntax,
        IForeignKeyCascadeSyntax<ICreateTableColumnOptionSyntax, ICreateTableColumnOptionForeignKeyCascadeSyntax>
    {

    }
}