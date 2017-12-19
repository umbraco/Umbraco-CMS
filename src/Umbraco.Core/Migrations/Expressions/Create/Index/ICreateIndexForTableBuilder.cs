namespace Umbraco.Core.Migrations.Expressions.Create.Index
{
    public interface ICreateIndexForTableBuilder : IFluentBuilder
    {
        ICreateIndexOnColumnBuilder OnTable(string tableName);
    }
}
