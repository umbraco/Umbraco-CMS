namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Table
{
    public interface ICreateTableWithColumnSyntax : IFluentSyntax
    {
        ICreateTableColumnAsTypeSyntax WithColumn(string name);
    }
}