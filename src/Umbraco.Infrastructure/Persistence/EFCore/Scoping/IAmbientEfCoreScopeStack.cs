using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

/// <summary>
/// Manages a stack of ambient EF Core scopes for thread-safe scope tracking.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal interface IAmbientEFCoreScopeStack<TDbContext> : IEFCoreScopeAccessor<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Gets the current ambient scope at the top of the stack.
    /// </summary>
    new IEfCoreScope<TDbContext>? AmbientScope { get; }

    /// <summary>
    /// Removes and returns the scope at the top of the stack.
    /// </summary>
    /// <returns>The scope that was removed from the top of the stack.</returns>
    IEfCoreScope<TDbContext> Pop();

    /// <summary>
    /// Pushes a scope onto the top of the stack.
    /// </summary>
    /// <param name="scope">The scope to push onto the stack.</param>
    void Push(IEfCoreScope<TDbContext> scope);
}
