namespace Umbraco.Core.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexColumnOptionsSyntax : IFluentSyntax
    {
        ICreateIndexOnColumnSyntax Ascending();
        ICreateIndexOnColumnSyntax Descending();
        ICreateIndexOnColumnSyntax Unique();
    }
}
