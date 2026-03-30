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
    /// Gets the <see cref="MapperCollectionBuilder"/> for registering and configuring mappers.
    /// </summary>
    /// <param name="builder">The Umbraco builder instance.</param>
    /// <returns>The <see cref="MapperCollectionBuilder"/> instance.</returns>
    public static MapperCollectionBuilder Mappers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<MapperCollectionBuilder>();

    /// <summary>
    /// Gets the collection builder for NPoco mappers.
    /// </summary>
    /// <param name="builder">The Umbraco builder instance.</param>
    /// <returns>An <see cref="NPocoMapperCollectionBuilder"/> for configuring NPoco mappers.</returns>
    public static NPocoMapperCollectionBuilder NPocoMappers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<NPocoMapperCollectionBuilder>();

    /// <summary>
    /// Gets the collection builder for package migration plans.
    /// </summary>
    /// <param name="builder">The Umbraco builder instance.</param>
    /// <returns>The <see cref="PackageMigrationPlanCollectionBuilder"/> for package migration plans.</returns>
    public static PackageMigrationPlanCollectionBuilder PackageMigrationPlans(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<PackageMigrationPlanCollectionBuilder>();

    /// <summary>
    /// Returns the collection builder for runtime mode validators.
    /// </summary>
    /// <param name="builder">The Umbraco builder instance.</param>
    /// <returns>The <see cref="RuntimeModeValidatorCollectionBuilder"/> for configuring runtime mode validators.</returns>
    public static RuntimeModeValidatorCollectionBuilder RuntimeModeValidators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<RuntimeModeValidatorCollectionBuilder>();
}
