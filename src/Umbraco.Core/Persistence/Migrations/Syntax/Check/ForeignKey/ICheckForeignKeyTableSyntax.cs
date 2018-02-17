namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public interface ICheckForeignKeyTableSyntax<T> where T : IFluentSyntax
    {
        T WithColumn(string columnName);
        T WithColumns(string[] columnNames);
    }
}
