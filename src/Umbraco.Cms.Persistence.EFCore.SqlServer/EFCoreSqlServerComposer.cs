using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

/// <summary>
/// Automatically adds SQL Server EF Core support to Umbraco when this project is referenced.
/// </summary>
public class EFCoreSqlServerComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder.AddUmbracoEFCoreSqlServerSupport();
}
