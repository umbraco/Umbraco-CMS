namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckConstraintOptionSyntax : IFluentSyntax
    {
        ICheckColumnConstraintOptionSyntax OnColumn(string columnName);
        ICheckColumnConstraintOptionSyntax OnColumns(string[] columnNames);
        ICheckTableConstraintOptionSyntax OnTable(string tableName);
    }
}
