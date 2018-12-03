namespace Umbraco.Core.Migrations.Expressions.Create.Index
{
    public interface ICreateIndexColumnOptionsBuilder : IFluentBuilder
    {
        ICreateIndexOnColumnBuilder Ascending();
        ICreateIndexOnColumnBuilder Descending();
        ICreateIndexOnColumnBuilder Unique();
    }
}
