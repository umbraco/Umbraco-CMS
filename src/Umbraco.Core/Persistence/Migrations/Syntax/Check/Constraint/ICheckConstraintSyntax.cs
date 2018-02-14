namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckConstraintSyntax : IFluentSyntax
    {
        ICheckColumnsConstraintOptionSyntax OnColumn(string columnName);
        ICheckColumnsConstraintOptionSyntax OnColumns(string[] columnNames);
        ICheckTableConstraintOptionSyntax OnTable(string tableName);
    }
}
