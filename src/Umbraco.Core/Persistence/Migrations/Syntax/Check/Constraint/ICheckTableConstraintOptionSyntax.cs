namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckTableConstraintOptionSyntax : ICheckExistsSyntax
    {
        ICheckExistsSyntax WithColumn(string columnName);
        ICheckExistsSyntax WithColumns(string[] columnNames);
    }
}
