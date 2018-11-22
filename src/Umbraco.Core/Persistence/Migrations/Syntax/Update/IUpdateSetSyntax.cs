namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
{
    public interface IUpdateSetSyntax
    {
        IUpdateWhereSyntax Set(object dataAsAnonymousType);
    }
}