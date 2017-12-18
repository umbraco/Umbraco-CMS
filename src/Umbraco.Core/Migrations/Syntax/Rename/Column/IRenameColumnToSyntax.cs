namespace Umbraco.Core.Migrations.Syntax.Rename.Column
{
    public interface IRenameColumnToSyntax : IFluentSyntax
    {
        void To(string name);
    }
}
