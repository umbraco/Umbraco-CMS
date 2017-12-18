namespace Umbraco.Core.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexForTableSyntax : IFluentSyntax
    {
        ICreateIndexOnColumnSyntax OnTable(string tableName);
    }
}
