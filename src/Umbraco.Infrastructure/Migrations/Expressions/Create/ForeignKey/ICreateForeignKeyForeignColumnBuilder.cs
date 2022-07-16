namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

public interface ICreateForeignKeyForeignColumnBuilder : IFluentBuilder
{
    ICreateForeignKeyToTableBuilder ForeignColumn(string column);

    ICreateForeignKeyToTableBuilder ForeignColumns(params string[] columns);
}
