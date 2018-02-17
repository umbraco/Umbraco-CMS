namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public interface ICheckForeignKeyForFromTableOptionSyntax : ICheckOptionSyntax
    {
        ICheckForeignKeyForToTableSyntax ToTable(string tableName);
    }
}
