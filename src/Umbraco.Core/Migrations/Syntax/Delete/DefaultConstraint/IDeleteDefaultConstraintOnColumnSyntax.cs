namespace Umbraco.Core.Migrations.Syntax.Delete.DefaultConstraint
{
    public interface IDeleteDefaultConstraintOnColumnSyntax : IFluentSyntax
    {
        void OnColumn(string columnName);
    }
}
