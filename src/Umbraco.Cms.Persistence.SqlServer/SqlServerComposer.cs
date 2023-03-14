using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Persistence.SqlServer;

/// <summary>
///     Automatically adds SQL Server support to Umbraco when this project is referenced.
/// </summary>
public class SqlServerComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder.AddUmbracoSqlServerSupport();
}
