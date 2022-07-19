using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to the <see cref="IUmbracoBuilder" /> class.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Gets the mappers collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static MapperCollectionBuilder? Mappers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<MapperCollectionBuilder>();

    public static NPocoMapperCollectionBuilder? NPocoMappers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<NPocoMapperCollectionBuilder>();

    /// <summary>
    ///     Gets the package migration plans collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static PackageMigrationPlanCollectionBuilder? PackageMigrationPlans(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<PackageMigrationPlanCollectionBuilder>();
}
