namespace Umbraco.Core.Migrations.Syntax.Update
{
    public interface IUpdateBuilder : IFluentSyntax
    {
        IUpdateSetSyntax Table(string tableName);
    }
}
