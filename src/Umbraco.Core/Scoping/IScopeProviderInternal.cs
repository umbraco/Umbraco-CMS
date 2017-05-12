namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Provides additional, internal scope provider functionnalities.
    /// </summary>
    /// <remarks>Extends <see cref="IScopeProvider"/> with internal features.</remarks>
    internal interface IScopeProviderInternal : IScopeProvider // fixme - define what's internal and why
    {
        /// <summary>
        /// Gets the ambient context.
        /// </summary>
        /// <remarks>May be null.</remarks>
        ScopeContext AmbientContext { get; }

        /// <summary>
        /// Gets the ambient scope.
        /// </summary>
        /// <remarks>May be null.</remarks>
        IScopeInternal AmbientScope { get; }

        /// <summary>
        /// Resets the ambient scope.
        /// </summary>
        /// <remarks>Resets the ambient scope (not completed anymore) and disposes the
        /// entire scopes chain until there is no more scopes.</remarks>
        void Reset();
    }
}
