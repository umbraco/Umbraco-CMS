namespace Umbraco.Core.Migrations.Syntax.Update
{
    public interface IUpdateWhereSyntax
    {
        void Where(object dataAsAnonymousType);
        void AllRows();
    }
}
