namespace Umbraco.Core.Migrations.Syntax.Insert
{
    public interface IInsertBuilder : IFluentSyntax
    {
        IInsertDataSyntax IntoTable(string tableName);
    }
}
