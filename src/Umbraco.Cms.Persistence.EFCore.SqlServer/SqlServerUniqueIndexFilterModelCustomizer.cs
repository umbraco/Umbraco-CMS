using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

/// <summary>
/// Removes the <c>IS NOT NULL</c> filter the SQL Server provider adds by convention to unique indexes over
/// nullable columns, so the model describes Umbraco's unique indexes the way <c>SqlServerSyntaxProvider</c>
/// creates them: without a filter.
/// </summary>
/// <remarks>
/// Only filters the provider convention produced are removed; a filter a configuration sets explicitly is kept.
/// Only Umbraco's own entity types are affected. Entity types other packages add to the model, such as the
/// OpenIddict ones, keep whatever filter their own migrations created.
/// </remarks>
public class SqlServerUniqueIndexFilterModelCustomizer : IEFCoreModelCustomizer
{
    /// <inheritdoc />
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLServer;

    /// <inheritdoc />
    public void Apply(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.Assembly != typeof(UmbracoDbContext).Assembly)
            {
                continue;
            }

            foreach (IMutableIndex index in entityType.GetIndexes())
            {
                if (((IConventionIndex)index).GetFilterConfigurationSource() == ConfigurationSource.Convention)
                {
                    index.SetFilter(null);
                }
            }
        }
    }
}
