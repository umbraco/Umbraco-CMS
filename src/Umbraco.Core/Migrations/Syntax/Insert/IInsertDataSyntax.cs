namespace Umbraco.Core.Migrations.Syntax.Insert
{
    public interface IInsertDataSyntax : IFluentSyntax
    {
        IInsertDataSyntax EnableIdentityInsert();
        IInsertDataSyntax Row(object dataAsAnonymousType);
    }
}
