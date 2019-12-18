namespace Umbraco.Core.Migrations.Expressions.Create.ForeignKey
{
    public interface ICreateForeignKeyForeignColumnBuilder : IFluentBuilder
    {
        ICreateForeignKeyToTableBuilder ForeignColumn(string column);
        ICreateForeignKeyToTableBuilder ForeignColumns(params string[] columns);
    }
}
