namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey
{
    public interface ICreateForeignKeyForeignColumnSyntax : IFluentSyntax
    {
        ICreateForeignKeyToTableSyntax ForeignColumn(string column);
        ICreateForeignKeyToTableSyntax ForeignColumns(params string[] columns);
    }
}