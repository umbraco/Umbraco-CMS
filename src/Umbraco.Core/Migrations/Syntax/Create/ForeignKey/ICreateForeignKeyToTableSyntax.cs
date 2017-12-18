namespace Umbraco.Core.Migrations.Syntax.Create.ForeignKey
{
    public interface ICreateForeignKeyToTableSyntax : IFluentSyntax
    {
        ICreateForeignKeyPrimaryColumnSyntax ToTable(string table);
    }
}
