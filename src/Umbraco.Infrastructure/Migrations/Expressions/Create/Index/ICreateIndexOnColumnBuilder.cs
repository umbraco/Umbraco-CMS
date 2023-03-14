using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;

public interface ICreateIndexOnColumnBuilder : IFluentBuilder, IExecutableBuilder
{
    /// <summary>
    ///     Specifies the index column.
    /// </summary>
    ICreateIndexColumnOptionsBuilder OnColumn(string columnName);

    /// <summary>
    ///     Specifies options.
    /// </summary>
    ICreateIndexOptionsBuilder WithOptions();
}
