namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Constraint
{
    public interface ICreateConstraintOnTableSyntax : IFluentSyntax
    {
        ICreateConstraintColumnsSyntax OnTable(string tableName);
    }
}