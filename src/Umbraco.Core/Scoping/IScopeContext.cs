namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Represents a scope context.
/// </summary>
/// <remarks>
///     A scope context can enlist objects that will be attached to the scope, and available
///     for the duration of the scope. In addition, it can enlist actions, that will run when the
///     scope is exiting, and after the database transaction has been committed.
/// </remarks>
public interface IScopeContext : IInstanceIdentifiable
{
    /// <summary>
    ///     Enlists an action.
    /// </summary>
    /// <param name="key">The action unique identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="priority">The optional action priority (default is 100, lower runs first).</param>
    /// <remarks>
    ///     <para>It is ok to enlist multiple action with the same key but only the first one will run.</para>
    ///     <para>The action boolean parameter indicates whether the scope completed or not.</para>
    /// </remarks>
    void Enlist(string key, Action<bool> action, int priority = 100);

    /// <summary>
    ///     Enlists an object and action.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="key">The object unique identifier.</param>
    /// <param name="creator">A function providing the object.</param>
    /// <param name="action">The optional action.</param>
    /// <param name="priority">The optional action priority (default is 100, lower runs first).</param>
    /// <returns>The object.</returns>
    /// <remarks>
    ///     <para>
    ///         On the first time an object is enlisted with a given key, the object is actually
    ///         created. Next calls just return the existing object. It is ok to enlist multiple objects
    ///         and action with the same key but only the first one is used, the others are ignored.
    ///     </para>
    ///     <para>The action boolean parameter indicates whether the scope completed or not.</para>
    /// </remarks>
    T? Enlist<T>(string key, Func<T> creator, Action<bool, T?>? action = null, int priority = 100);

    /// <summary>
    ///     Gets an enlisted object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="key">The object unique identifier.</param>
    /// <returns>The enlisted object, if any, else the default value.</returns>
    T? GetEnlisted<T>(string key);

    void ScopeExit(bool completed);
}
