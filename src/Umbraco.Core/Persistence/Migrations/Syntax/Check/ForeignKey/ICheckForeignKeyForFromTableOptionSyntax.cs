namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public interface ICheckForeignKeyForFromTableOptionSyntax : IFluentSyntax
    {
        ICheckForeignKeyForToTableSyntax ToTable(string tableName);
    }
}
