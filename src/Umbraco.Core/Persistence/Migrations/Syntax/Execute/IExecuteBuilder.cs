namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute
{
    public interface IExecuteBuilder : IFluentSyntax
    {
        void Sql(string sqlStatement);
    }
}