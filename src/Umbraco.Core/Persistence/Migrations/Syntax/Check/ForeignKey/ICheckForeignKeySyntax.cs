namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.ForeignKey
{
    public interface ICheckForeignKeySyntax : ICheckOptionSyntax
    {
        ICheckForeignKeyForFromTableSyntax FromTable(string tableName);
    }
}
