namespace Umbraco.Core.Migrations.Expressions.Create.Index
{
    public interface ICreateIndexOptionsBuilder : IFluentBuilder
    {
        ICreateIndexOnColumnBuilder Unique();
        ICreateIndexOnColumnBuilder NonClustered();
        ICreateIndexOnColumnBuilder Clustered();
    }
}
