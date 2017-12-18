namespace Umbraco.Core.Migrations.Syntax.Delete.Index
{
    public interface IDeleteIndexForTableSyntax : IFluentSyntax
    {
        IDeleteIndexOnColumnSyntax OnTable(string tableName);
    }
}
