namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Table
{
    public interface ICreateTableColumnOptionSyntax :
        IColumnOptionSyntax<ICreateTableColumnOptionSyntax, ICreateTableColumnOptionForeignKeyCascadeSyntax>,
        ICreateTableWithColumnSyntax
    {

    }
}