namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckConstraintOptionSyntax : IFluentSyntax
    {
        ICheckTableConstraintOptionSyntax OnTable(string tableName);
        ICheckColumnConstraintOptionSyntax OnColumn(string columnName);
    }
}
