namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Provices scopes.
    /// </summary>
    /// <remarks>Extends <see cref="IScopeProvider"/> with internal features.</remarks>
    internal interface IScopeProviderInternal : IScopeProvider
    {
        /// <summary>
        /// Gets the ambient scope.
        /// </summary>
        IScope AmbientScope { get; }

        // fixme
        IScope AmbientOrNoScope { get; }

        /// <summary>
        /// Creates a <see cref="NoScope"/> instance.
        /// </summary>
        /// <returns>The created ambient scope.</returns>
        /// <remarks>
        /// <para>The created scope becomes the ambient scope.</para>
        /// <para>If an ambient scope already exists, throws.</para>
        /// <para>The <see cref="NoScope"/> instance can be eventually replaced by a real instance.</para>
        /// </remarks>
        //IScope CreateNoScope();
    }
}
