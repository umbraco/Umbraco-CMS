namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyFromTableSyntax : IFluentSyntax
    {
        IDeleteForeignKeyForeignColumnSyntax FromTable(string foreignTableName);
    }
}