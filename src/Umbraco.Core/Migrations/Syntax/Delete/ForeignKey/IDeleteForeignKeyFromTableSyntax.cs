namespace Umbraco.Core.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyFromTableSyntax : IFluentSyntax
    {
        IDeleteForeignKeyForeignColumnSyntax FromTable(string foreignTableName);
    }
}
