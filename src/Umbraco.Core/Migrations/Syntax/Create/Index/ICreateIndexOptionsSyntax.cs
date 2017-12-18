namespace Umbraco.Core.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexOptionsSyntax : IFluentSyntax
    {
        ICreateIndexOnColumnSyntax Unique();
        ICreateIndexOnColumnSyntax NonClustered();
        ICreateIndexOnColumnSyntax Clustered();
    }
}
