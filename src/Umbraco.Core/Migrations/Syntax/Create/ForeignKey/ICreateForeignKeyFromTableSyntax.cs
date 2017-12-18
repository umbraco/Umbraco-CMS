namespace Umbraco.Core.Migrations.Syntax.Create.ForeignKey
{
    public interface ICreateForeignKeyFromTableSyntax : IFluentSyntax
    {
        ICreateForeignKeyForeignColumnSyntax FromTable(string table);
    }
}
