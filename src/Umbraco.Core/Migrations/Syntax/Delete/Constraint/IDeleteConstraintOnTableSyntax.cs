namespace Umbraco.Core.Migrations.Syntax.Delete.Constraint
{
    public interface IDeleteConstraintOnTableSyntax : IFluentSyntax
    {
        void FromTable(string tableName);
    }
}
