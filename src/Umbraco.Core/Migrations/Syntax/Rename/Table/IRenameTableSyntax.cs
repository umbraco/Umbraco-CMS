namespace Umbraco.Core.Migrations.Syntax.Rename.Table
{
    public interface IRenameTableSyntax : IFluentSyntax
    {
        void To(string name);
    }
}
