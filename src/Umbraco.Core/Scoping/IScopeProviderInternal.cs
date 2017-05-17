namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Provides scopes.
    /// </summary>
    /// <remarks>Extends <see cref="IScopeProvider"/> with internal features.</remarks>
    internal interface IScopeProviderInternal : IScopeProvider
    {
        /// <summary>
        /// Gets the ambient context.
        /// </summary>
        ScopeContext AmbientContext { get; }

        /// <summary>
        /// Gets the ambient scope.
        /// </summary>
        IScopeInternal AmbientScope { get; }

        /// <summary>
        /// Gets the ambient scope if any, else creates and returns a <see cref="NoScope"/>.
        /// </summary>
        IScopeInternal GetAmbientOrNoScope();

        /// <summary>
        /// Resets the ambient scope.
        /// </summary>
        /// <remarks>Resets the ambient scope (not completed anymore) and disposes the 
        /// entire scopes chain until there is no more scopes.</remarks>
        void Reset();
    }
}
