namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Column
{
    public interface IDeleteColumnFromTableSyntax : IFluentSyntax
    {
        void FromTable(string tableName);
        IDeleteColumnFromTableSyntax Column(string columnName);
    }
}