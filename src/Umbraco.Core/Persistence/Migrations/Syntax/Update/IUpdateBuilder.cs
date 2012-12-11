namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
{
    public interface IUpdateBuilder : IFluentSyntax
    {
        IUpdateSetSyntax Table(string tableName);
    }
}