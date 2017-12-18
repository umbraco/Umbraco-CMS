namespace Umbraco.Core.Migrations.Syntax.Delete.DefaultConstraint
{
    public interface IDeleteDefaultConstraintOnTableSyntax : IFluentSyntax
    {
        IDeleteDefaultConstraintOnColumnSyntax OnTable(string tableName);
    }
}
