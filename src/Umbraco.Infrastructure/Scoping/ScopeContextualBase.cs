namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Provides a base class for scope contextual objects.
/// </summary>
/// <remarks>
///     <para>
///         A scope contextual object is enlisted in the current scope context,
///         if any, and released when the context exists. It must be used in a 'using'
///         block, and will be released when disposed, if not part of a scope.
///     </para>
/// </remarks>
public abstract class ScopeContextualBase : IDisposable
{
    private bool _scoped;

    /// <summary>
        /// Gets a contextual object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="scopeProvider">A scope provider.</param>
        /// <param name="key">A context key for the object.</param>
        /// <param name="ctor">A function producing the contextual object.</param>
        /// <returns>The contextual object.</returns>
        /// <remarks>
        /// <para></para>
        /// </remarks>
        [Obsolete("Please use the Get method that accepts an Umbraco.Cms.Core.Scoping.ICoreScopeProvider instead.")]
        public static T? Get<T>(IScopeProvider scopeProvider, string key, Func<bool, T> ctor)
            where T : ScopeContextualBase
            => Get<T>((ICoreScopeProvider)scopeProvider, key, ctor);

        /// <summary>
    ///     Gets a contextual object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="scopeProvider">A scope provider.</param>
    /// <param name="key">A context key for the object.</param>
    /// <param name="ctor">A function producing the contextual object.</param>
    /// <returns>The contextual object.</returns>
    /// <remarks>
    ///     <para></para>
    /// </remarks>
    public static T? Get<T>(ICoreScopeProvider scopeProvider, string key, Func<bool, T> ctor)
        where T : ScopeContextualBase
    {
        // no scope context = create a non-scoped object
        IScopeContext? scopeContext = scopeProvider.Context;
        if (scopeContext == null)
        {
            return ctor(false);
        }

        // create & enlist the scoped object
        T? w = scopeContext.Enlist(
            "ScopeContextualBase_" + key,
            () => ctor(true),
            (completed, item) => { item?.Release(completed); });

        if (w is not null)
        {
            w._scoped = true;
        }

        return w;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>If not scoped, then this releases the contextual object.</para>
    /// </remarks>
    public void Dispose()
    {
        if (_scoped == false)
        {
            Release(true);
        }
    }

    /// <summary>
    ///     Releases the contextual object.
    /// </summary>
    /// <param name="completed">A value indicating whether the scoped operation completed.</param>
    public abstract void Release(bool completed);
}
