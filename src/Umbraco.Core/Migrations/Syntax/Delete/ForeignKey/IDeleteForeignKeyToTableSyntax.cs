namespace Umbraco.Core.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyToTableSyntax : IFluentSyntax
    {
        IDeleteForeignKeyPrimaryColumnSyntax ToTable(string table);
    }
}
