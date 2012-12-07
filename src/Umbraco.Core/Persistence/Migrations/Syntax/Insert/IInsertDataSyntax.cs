namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert
{
    public interface IInsertDataSyntax : IFluentSyntax
    {
        IInsertDataSyntax Row(object dataAsAnonymousType);
    }
}