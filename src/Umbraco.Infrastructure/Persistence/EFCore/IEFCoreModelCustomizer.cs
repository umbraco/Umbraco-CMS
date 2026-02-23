using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
///     Non-generic interface for resolving all registered EF Core model customizers from DI.
/// </summary>
public interface IEFCoreModelCustomizer
{
    /// <summary>Gets the entity type this customizer applies to.</summary>
    Type EntityType { get; }

    /// <summary>Applies provider-specific model configuration.</summary>
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
    /// <summary>Applies provider-specific configuration for the entity type.</summary>
    void Customize(EntityTypeBuilder<TEntity> builder);

    // Default implementations — providers do not override these
    Type IEFCoreModelCustomizer.EntityType => typeof(TEntity);

    void IEFCoreModelCustomizer.Apply(ModelBuilder modelBuilder)
    {
        if (modelBuilder.Model.FindEntityType(typeof(TEntity)) is null)
        {
            throw new InvalidOperationException("The context does not contain the entity type " + typeof(TEntity).FullName);
        }

        Customize(modelBuilder.Entity<TEntity>());
    }
}
