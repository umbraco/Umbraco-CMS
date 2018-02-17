namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Index
{
    public interface ICheckIndexSyntax : ICheckIndexOptionSyntax
    {
        ICheckIndexForTableSyntax OnTable(string tableName);
    }
}
