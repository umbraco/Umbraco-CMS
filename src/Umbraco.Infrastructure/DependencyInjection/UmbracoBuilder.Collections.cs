using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Runtime;

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

    /// <summary>
    ///     Gets the NPoco mappers collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static NPocoMapperCollectionBuilder? NPocoMappers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<NPocoMapperCollectionBuilder>();

    /// <summary>
    ///     Gets the package migration plans collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static PackageMigrationPlanCollectionBuilder? PackageMigrationPlans(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<PackageMigrationPlanCollectionBuilder>();

    /// <summary>
    ///     Gets the runtime mode validators collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static RuntimeModeValidatorCollectionBuilder RuntimeModeValidators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<RuntimeModeValidatorCollectionBuilder>();
}
