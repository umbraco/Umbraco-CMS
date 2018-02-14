namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckTableConstraintOptionSyntax : ICheckOptionSyntax
    {
        ICheckOptionSyntax AndColumn(string columnName);
        ICheckOptionSyntax AndColumns(string[] columnNames);
    }
}
