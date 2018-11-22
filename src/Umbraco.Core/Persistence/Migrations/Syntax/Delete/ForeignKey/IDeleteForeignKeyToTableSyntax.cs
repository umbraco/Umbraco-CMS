namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyToTableSyntax : IFluentSyntax
    {
        IDeleteForeignKeyPrimaryColumnSyntax ToTable(string table);
    }
}