namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Index
{
    public interface ICheckIndexForTableSyntax : ICheckIndexOptionSyntax
    {
        ICheckIndexOptionSyntax AndColumn(string columnName);
        ICheckIndexOptionSyntax AndColumns(string[] columnNames);
    }
}
