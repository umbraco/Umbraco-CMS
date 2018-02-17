namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Index
{
    public interface ICheckIndexForTableSyntax : ICheckIndexOptionSyntax
    {
        ICheckIndexOptionSyntax WithColumn(string columnName);
        ICheckIndexOptionSyntax WithColumns(string[] columnNames);
    }
}
