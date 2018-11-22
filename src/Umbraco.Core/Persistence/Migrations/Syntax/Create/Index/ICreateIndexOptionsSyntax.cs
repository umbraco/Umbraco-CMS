namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Index
{
    public interface ICreateIndexOptionsSyntax : IFluentSyntax
    {
        ICreateIndexOnColumnSyntax Unique();
        ICreateIndexOnColumnSyntax NonClustered();
        ICreateIndexOnColumnSyntax Clustered();
    }
}