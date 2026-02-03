using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Scoping;
using CoreEFCoreScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

/// <summary>
/// Provides access to the current ambient EF Core scope.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal sealed class EFCoreScopeAccessor<TDbContext> : IEFCoreScopeAccessor<TDbContext> where TDbContext : DbContext
{
    private readonly IAmbientEFCoreScopeStack<TDbContext> _ambientEfCoreScopeStack;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScopeAccessor{TDbContext}"/> class.
    /// </summary>
    /// <param name="ambientEfCoreScopeStack">The ambient scope stack.</param>
    public EFCoreScopeAccessor(IAmbientEFCoreScopeStack<TDbContext> ambientEfCoreScopeStack) => _ambientEfCoreScopeStack = ambientEfCoreScopeStack;

    /// <summary>
    /// Gets the current ambient scope as a concrete EFCoreScope instance.
    /// </summary>
    public EFCoreScope<TDbContext>? AmbientScope => (EFCoreScope<TDbContext>?)_ambientEfCoreScopeStack.AmbientScope;

    /// <inheritdoc />
    IEfCoreScope<TDbContext>? IEFCoreScopeAccessor<TDbContext>.AmbientScope => _ambientEfCoreScopeStack.AmbientScope;

    /// <inheritdoc />
    ICoreScope? CoreEFCoreScopeAccessor.AmbientScope => _ambientEfCoreScopeStack.AmbientScope;
}
