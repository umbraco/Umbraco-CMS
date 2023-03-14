namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

public interface ICreateForeignKeyFromTableBuilder : IFluentBuilder
{
    ICreateForeignKeyForeignColumnBuilder FromTable(string table);
}
