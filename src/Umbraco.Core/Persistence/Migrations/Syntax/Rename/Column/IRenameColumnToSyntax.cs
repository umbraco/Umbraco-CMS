namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename.Column
{
    public interface IRenameColumnToSyntax : IFluentSyntax
    {
        void To(string name);
    }
}