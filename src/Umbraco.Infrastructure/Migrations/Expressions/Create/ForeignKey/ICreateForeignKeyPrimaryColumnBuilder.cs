namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

public interface ICreateForeignKeyPrimaryColumnBuilder : IFluentBuilder
{
    ICreateForeignKeyCascadeBuilder PrimaryColumn(string column);

    ICreateForeignKeyCascadeBuilder PrimaryColumns(params string[] columns);
}
