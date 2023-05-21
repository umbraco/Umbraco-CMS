namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

public interface ICreateForeignKeyToTableBuilder : IFluentBuilder
{
    ICreateForeignKeyPrimaryColumnBuilder ToTable(string table);
}
