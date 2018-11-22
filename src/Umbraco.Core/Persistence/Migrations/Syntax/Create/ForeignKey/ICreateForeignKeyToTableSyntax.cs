namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.ForeignKey
{
    public interface ICreateForeignKeyToTableSyntax : IFluentSyntax
    {
        ICreateForeignKeyPrimaryColumnSyntax ToTable(string table);
    }
}