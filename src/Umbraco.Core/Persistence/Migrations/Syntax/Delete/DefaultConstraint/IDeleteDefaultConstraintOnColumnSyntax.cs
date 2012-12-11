namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.DefaultConstraint
{
    public interface IDeleteDefaultConstraintOnColumnSyntax : IFluentSyntax
    {
        void OnColumn(string columnName);
    }
}