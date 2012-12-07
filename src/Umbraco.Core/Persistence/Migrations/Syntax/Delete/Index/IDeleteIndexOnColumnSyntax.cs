namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index
{
    public interface IDeleteIndexOnColumnSyntax : IFluentSyntax
    {
        void OnColumn(string columnName);
        void OnColumns(params string[] columnNames);
    }
}