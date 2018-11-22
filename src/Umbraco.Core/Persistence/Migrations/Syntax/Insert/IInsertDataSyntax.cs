namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert
{
    public interface IInsertDataSyntax : IFluentSyntax
    {
        IInsertDataSyntax EnableIdentityInsert();
        IInsertDataSyntax Row(object dataAsAnonymousType);
    }
}