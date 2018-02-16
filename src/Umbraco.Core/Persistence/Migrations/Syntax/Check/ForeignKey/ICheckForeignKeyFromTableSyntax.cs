namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public interface ICheckForeignKeyFromTableSyntax : ICheckOptionSyntax
    {
        ICheckForeignKeyToTableSyntax ToTable(string tableName);
    }
}
