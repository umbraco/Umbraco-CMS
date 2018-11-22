namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert
{
    public interface IInsertBuilder : IFluentSyntax
    {
        IInsertDataSyntax IntoTable(string tableName);
    }
}