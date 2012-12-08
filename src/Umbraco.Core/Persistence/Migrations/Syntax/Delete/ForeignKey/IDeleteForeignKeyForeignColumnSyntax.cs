namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyForeignColumnSyntax : IFluentSyntax
    {
        IDeleteForeignKeyToTableSyntax ForeignColumn(string column);
        IDeleteForeignKeyToTableSyntax ForeignColumns(params string[] columns);
    }
}