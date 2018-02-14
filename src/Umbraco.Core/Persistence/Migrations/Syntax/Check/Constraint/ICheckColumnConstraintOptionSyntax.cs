namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckColumnConstraintOptionSyntax : ICheckOptionSyntax
    {
        ICheckOptionSyntax AndTable(string tableName);
    }
}
