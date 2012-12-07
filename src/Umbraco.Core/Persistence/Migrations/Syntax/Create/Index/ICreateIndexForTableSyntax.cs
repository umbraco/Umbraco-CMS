namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexForTableSyntax : IFluentSyntax
    {
        ICreateIndexOnColumnSyntax OnTable(string tableName);
    }
}