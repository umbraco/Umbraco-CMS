namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckColumnsConstraintOptionSyntax : ICheckExistsSyntax
    {
        ICheckExistsSyntax AndTable(string tableName);
    }
}
