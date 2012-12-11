namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexColumnOptionsSyntax : IFluentSyntax
    {
        ICreateIndexOnColumnSyntax Ascending();
        ICreateIndexOnColumnSyntax Descending();
        ICreateIndexOnColumnSyntax Unique();
    }
}