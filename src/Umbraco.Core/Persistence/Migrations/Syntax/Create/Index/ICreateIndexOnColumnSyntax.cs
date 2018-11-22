namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexOnColumnSyntax : IFluentSyntax
    {
        ICreateIndexColumnOptionsSyntax OnColumn(string columnName);
        ICreateIndexOptionsSyntax WithOptions();
    }
}