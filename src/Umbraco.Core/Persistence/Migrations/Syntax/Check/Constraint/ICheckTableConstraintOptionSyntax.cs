namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckTableConstraintOptionSyntax : ICheckOptionSyntax
    {
        ICheckOptionSyntax WithColumn(string columnName);
        ICheckOptionSyntax WithColumns(string[] columnNames);
    }
}
