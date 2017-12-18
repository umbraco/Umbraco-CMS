namespace Umbraco.Core.Migrations.Syntax.Create.Table
{
    public interface ICreateTableColumnOptionSyntax :
        IColumnOptionSyntax<ICreateTableColumnOptionSyntax, ICreateTableColumnOptionForeignKeyCascadeSyntax>,
        ICreateTableWithColumnSyntax
    {

    }
}
