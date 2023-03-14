namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Implements an un-ordered collection builder.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TCollection">The type of the collection.</typeparam>
/// <typeparam name="TItem">The type of the items.</typeparam>
/// <remarks>
///     <para>
///         A set collection builder is the most basic collection builder,
///         where items are not ordered.
///     </para>
/// </remarks>
public abstract class SetCollectionBuilderBase<TBuilder, TCollection, TItem> : CollectionBuilderBase<TBuilder, TCollection, TItem>
    where TBuilder : SetCollectionBuilderBase<TBuilder, TCollection, TItem>
    where TCollection : class, IBuilderCollection<TItem>
{
    protected abstract TBuilder This { get; }

    /// <summary>
    ///     Clears all types in the collection.
    /// </summary>
    /// <returns>The builder.</returns>
    public TBuilder Clear()
    {
        Configure(types => types.Clear());
        return This;
    }

    /// <summary>
    ///     Adds a type to the collection.
    /// </summary>
    /// <typeparam name="T">The type to append.</typeparam>
    /// <returns>The builder.</returns>
    public TBuilder Add<T>()
        where T : TItem
    {
        Configure(types =>
        {
            Type type = typeof(T);
            if (types.Contains(type))
            {
                types.Remove(type);
            }

            types.Add(type);
        });
        return This;
    }

    /// <summary>
    ///     Adds a type to the collection.
    /// </summary>
    /// <param name="type">The type to append.</param>
    /// <returns>The builder.</returns>
    public TBuilder Add(Type type)
    {
        Configure(types =>
        {
            EnsureType(type, "register");
            if (types.Contains(type))
            {
                types.Remove(type);
            }

            types.Add(type);
        });
        return This;
    }

    /// <summary>
    ///     Adds types to the collections.
    /// </summary>
    /// <param name="types">The types to append.</param>
    /// <returns>The builder.</returns>
    public TBuilder Add(IEnumerable<Type> types)
    {
        Configure(list =>
        {
            foreach (Type type in types)
            {
                // would be detected by CollectionBuilderBase when registering, anyways, but let's fail fast
                EnsureType(type, "register");
                if (list.Contains(type))
                {
                    list.Remove(type);
                }

                list.Add(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Removes a type from the collection.
    /// </summary>
    /// <typeparam name="T">The type to remove.</typeparam>
    /// <returns>The builder.</returns>
    public TBuilder Remove<T>()
        where T : TItem
    {
        Configure(types =>
        {
            Type type = typeof(T);
            if (types.Contains(type))
            {
                types.Remove(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Removes a type from the collection.
    /// </summary>
    /// <param name="type">The type to remove.</param>
    /// <returns>The builder.</returns>
    public TBuilder Remove(Type type)
    {
        Configure(types =>
        {
            EnsureType(type, "remove");
            if (types.Contains(type))
            {
                types.Remove(type);
            }
        });
        return This;
    }

    /// <summary>
    ///     Replaces a type in the collection.
    /// </summary>
    /// <typeparam name="TReplaced">The type to replace.</typeparam>
    /// <typeparam name="T">The type to insert.</typeparam>
    /// <returns>The builder.</returns>
    /// <remarks>Throws if the type to replace does not already belong to the collection.</remarks>
    public TBuilder Replace<TReplaced, T>()
        where TReplaced : TItem
        where T : TItem
    {
        Configure(types =>
        {
            Type typeReplaced = typeof(TReplaced);
            Type type = typeof(T);
            if (typeReplaced == type)
            {
                return;
            }

            var index = types.IndexOf(typeReplaced);
            if (index < 0)
            {
                throw new InvalidOperationException();
            }

            if (types.Contains(type))
            {
                types.Remove(type);
            }

            index = types.IndexOf(typeReplaced); // in case removing type changed index
            types.Insert(index, type);
            types.Remove(typeReplaced);
        });
        return This;
    }

    /// <summary>
    ///     Replaces a type in the collection.
    /// </summary>
    /// <param name="typeReplaced">The type to replace.</param>
    /// <param name="type">The type to insert.</param>
    /// <returns>The builder.</returns>
    /// <remarks>Throws if the type to replace does not already belong to the collection.</remarks>
    public TBuilder Replace(Type typeReplaced, Type type)
    {
        Configure(types =>
        {
            EnsureType(typeReplaced, "find");
            EnsureType(type, "register");

            if (typeReplaced == type)
            {
                return;
            }

            var index = types.IndexOf(typeReplaced);
            if (index < 0)
            {
                throw new InvalidOperationException();
            }

            if (types.Contains(type))
            {
                types.Remove(type);
            }

            index = types.IndexOf(typeReplaced); // in case removing type changed index
            types.Insert(index, type);
            types.Remove(typeReplaced);
        });
        return This;
    }
}
