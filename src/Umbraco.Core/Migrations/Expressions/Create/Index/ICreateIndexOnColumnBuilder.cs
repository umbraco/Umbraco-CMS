namespace Umbraco.Core.Migrations.Expressions.Create.Index
{
    public interface ICreateIndexOnColumnBuilder : IFluentBuilder
    {
        ICreateIndexColumnOptionsBuilder OnColumn(string columnName);
        ICreateIndexOptionsBuilder WithOptions();
    }
}
