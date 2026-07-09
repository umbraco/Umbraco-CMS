using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Scoping;

/// <summary>
/// Represents an interface for a stack that manages ambient scope contexts, typically used for handling nested or hierarchical scope states.
/// </summary>
public interface IAmbientScopeContextStack
{
    /// <summary>
    /// Gets the current ambient <see cref="IScopeContext"/>, representing the active scope context for the current execution environment.
    /// </summary>
    IScopeContext? AmbientContext { get; }

    /// <summary>
    /// Removes and returns the current <see cref="IScopeContext"/> from the top of the stack.
    /// </summary>
    /// <returns>The <see cref="IScopeContext"/> instance that was removed from the stack.</returns>
    IScopeContext Pop();

    /// <summary>
    /// Pushes the specified <see cref="IScopeContext"/> instance onto the ambient scope context stack.
    /// </summary>
    /// <param name="scope">The <see cref="IScopeContext"/> to push onto the stack.</param>
    void Push(IScopeContext scope);
}
