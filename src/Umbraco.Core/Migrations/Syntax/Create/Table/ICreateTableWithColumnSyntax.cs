namespace Umbraco.Core.Migrations.Syntax.Create.Table
{
    public interface ICreateTableWithColumnSyntax : IFluentSyntax
    {
        ICreateTableColumnAsTypeSyntax WithColumn(string name);
    }
}
