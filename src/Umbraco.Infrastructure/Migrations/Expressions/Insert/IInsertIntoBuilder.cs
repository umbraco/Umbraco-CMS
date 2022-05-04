using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert;

/// <summary>
///     Builds an Insert Into expression.
/// </summary>
public interface IInsertIntoBuilder : IFluentBuilder, IExecutableBuilder
{
    /// <summary>
    ///     Enables identity insert.
    /// </summary>
    IInsertIntoBuilder EnableIdentityInsert();

    /// <summary>
    ///     Specifies a row to be inserted.
    /// </summary>
    IInsertIntoBuilder Row(object dataAsAnonymousType);
}
