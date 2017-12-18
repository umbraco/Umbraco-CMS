namespace Umbraco.Core.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexOnColumnSyntax : IFluentSyntax
    {
        ICreateIndexColumnOptionsSyntax OnColumn(string columnName);
        ICreateIndexOptionsSyntax WithOptions();
    }
}
