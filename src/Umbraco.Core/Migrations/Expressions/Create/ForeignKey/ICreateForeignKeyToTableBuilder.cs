namespace Umbraco.Core.Migrations.Expressions.Create.ForeignKey
{
    public interface ICreateForeignKeyToTableBuilder : IFluentBuilder
    {
        ICreateForeignKeyPrimaryColumnBuilder ToTable(string table);
    }
}
