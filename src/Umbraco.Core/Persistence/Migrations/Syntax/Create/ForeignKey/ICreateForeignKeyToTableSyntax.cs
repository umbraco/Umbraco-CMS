namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey
{
    public interface ICreateForeignKeyToTableSyntax : IFluentSyntax
    {
        ICreateForeignKeyForeignColumnSyntax ToTable(string table);
    }
}