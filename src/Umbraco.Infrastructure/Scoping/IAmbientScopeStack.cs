namespace Umbraco.Cms.Infrastructure.Scoping;

internal interface IAmbientScopeStack : IScopeAccessor
{
    /// <summary>Removes and returns the current ambient scope from the stack.</summary>
    /// <returns>The current ambient <see cref="Umbraco.Cms.Infrastructure.Scoping.IScope"/> that was removed from the stack.</returns>
    IScope Pop();

    /// <summary>
    /// Pushes the specified scope onto the ambient scope stack.
    /// </summary>
    /// <param name="scope">The scope to push onto the stack.</param>
    void Push(IScope scope);
}
