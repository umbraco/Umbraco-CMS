namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Constraint
{
    public interface ICheckColumnsConstraintOptionSyntax : ICheckOptionSyntax
    {
        ICheckOptionSyntax AndTable(string tableName);
    }
}
