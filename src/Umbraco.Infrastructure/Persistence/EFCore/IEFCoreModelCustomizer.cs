using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
///     Non-generic interface for resolving all registered EF Core model customizers from DI.
/// </summary>
public interface IEFCoreModelCustomizer
{
    /// <summary>
    /// Gets the EF Core provider name this customizer targets, or <c>null</c> if it applies to all providers.
    /// </summary>
    /// <remarks>
    /// When non-null, <see cref="UmbracoDbContext"/> skips this customizer if the active
    /// database provider (from <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.ProviderName"/>)
    /// does not match this value (case-insensitive).
    /// Use the EF Core provider name, e.g. <c>"Microsoft.EntityFrameworkCore.Sqlite"</c> or
    /// <c>"Microsoft.EntityFrameworkCore.SqlServer"</c>.
    /// </remarks>
    string? ProviderName => null;

    /// <summary>
    /// Applies provider-specific model configuration.
    /// </summary>
    void Apply(ModelBuilder modelBuilder);
}

/// <summary>
///     Allows external packages to apply additional, provider-specific EF Core model
///     configuration for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type to configure.</typeparam>
/// <remarks>
///     Register implementations with
///     <c>builder.AddEFCoreModelCustomizer&lt;TCustomizer&gt;()</c>.
/// </remarks>
public interface IEFCoreModelCustomizer<TEntity> : IEFCoreModelCustomizer
    where TEntity : class
{
    /// <summary>
    /// Applies provider-specific configuration for the entity type.
    /// </summary>
    void Customize(EntityTypeBuilder<TEntity> builder);

    // Default implementation — providers do not override this
    /// <inheritdoc/>
    void IEFCoreModelCustomizer.Apply(ModelBuilder modelBuilder)
    {
        if (modelBuilder.Model.FindEntityType(typeof(TEntity)) is null)
        {
            throw new InvalidOperationException("The context does not contain the entity type " + typeof(TEntity).FullName);
        }

        Customize(modelBuilder.Entity<TEntity>());
    }
}
