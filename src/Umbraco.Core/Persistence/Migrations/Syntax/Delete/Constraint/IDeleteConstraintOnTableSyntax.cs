namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Constraint
{
    public interface IDeleteConstraintOnTableSyntax : IFluentSyntax
    {
        void FromTable(string tableName);
    }
}