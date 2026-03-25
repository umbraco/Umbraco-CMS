using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Provides access to the current ambient EF Core scope.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal sealed class EFCoreScopeAccessor<TDbContext> : IEFCoreScopeAccessor<TDbContext> where TDbContext : DbContext
{
    private readonly IAmbientEFCoreScopeStack<TDbContext> _ambientEFCoreScopeStack;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScopeAccessor{TDbContext}"/> class.
    /// </summary>
    /// <param name="ambientEFCoreScopeStack">The ambient scope stack.</param>
    public EFCoreScopeAccessor(IAmbientEFCoreScopeStack<TDbContext> ambientEFCoreScopeStack) => _ambientEFCoreScopeStack = ambientEFCoreScopeStack;

    /// <summary>
    /// Gets the current ambient scope as a concrete EFCoreScope instance.
    /// </summary>
    public EFCoreScope<TDbContext>? AmbientScope => (EFCoreScope<TDbContext>?)_ambientEFCoreScopeStack.AmbientScope;

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    IEfCoreScope<TDbContext>? IEFCoreScopeAccessor<TDbContext>.AmbientScope => (IEfCoreScope<TDbContext>?)_ambientEFCoreScopeStack.AmbientScope;
#pragma warning restore CS0618 // Type or member is obsolete
}
