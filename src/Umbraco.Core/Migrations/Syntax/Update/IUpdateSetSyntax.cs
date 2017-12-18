namespace Umbraco.Core.Migrations.Syntax.Update
{
    public interface IUpdateSetSyntax
    {
        IUpdateWhereSyntax Set(object dataAsAnonymousType);
    }
}
