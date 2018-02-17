namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public interface ICheckForeignKeySyntax : ICheckExistsSyntax
    {
        ICheckForeignKeyForFromTableSyntax FromTable(string tableName);
    }
}
