namespace Umbraco.Core.Migrations.Syntax.Rename.Column
{
    public interface IRenameColumnTableSyntax : IFluentSyntax
    {
        IRenameColumnToSyntax OnTable(string tableName);
    }
}
